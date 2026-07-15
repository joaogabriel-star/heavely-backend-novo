// Services/PontoService.cs
using Microsoft.EntityFrameworkCore;
using SistemaHEAVELYBackend.Data;
using SistemaHEAVELYBackend.DTOs.Ponto;
using SistemaHEAVELYBackend.Services.Interfaces;

namespace SistemaHEAVELYBackend.Services
{
    public class PontoService : IPontoService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public PontoService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ── ENTRADA ───────────────────────────────────────────────────────
        public async Task<PontoRespostaDTO> RegistrarEntradaAsync(int idUsuario, int idEvento)
        {
            // 1. Busca a alocação — usuário precisa estar confirmado neste evento
            var alocacao = await _context.Alocacoes
                .Include(a => a.IdEventoNavigation)
                .FirstOrDefaultAsync(a =>
                    a.IdUsuario == idUsuario &&
                    a.IdEvento  == idEvento  &&
                    a.StatusParticipacao == "Confirmado");

            if (alocacao == null)
                return Erro("Você não possui inscrição confirmada neste evento.");

            if (alocacao.CheckInTime != null)
                return Erro("Entrada já registrada anteriormente.");

            var evento      = alocacao.IdEventoNavigation;
            var agora       = DateTime.UtcNow;
            var inicioProva = evento.DataProva;

            // 2. Regra: não pode bater ponto mais de 2h antes da prova
            if (agora < inicioProva.AddHours(-2))
                return Erro($"O registro de entrada só é liberado a partir das " +
                            $"{inicioProva.AddHours(-2):HH:mm}.");

            // 3. Registra a hora real de chegada
            alocacao.CheckInTime = agora;
            alocacao.StatusParticipacao = "Presente";

            // 4. Hora que o CÁLCULO começa:
            //    - Se chegou antes → cálculo começa no horário oficial da prova
            //    - Se chegou depois → cálculo começa na hora de chegada (já atrasado)
            var horaInicioCalculo = agora < inicioProva ? inicioProva : agora;

            // Salva no banco
            await _context.SaveChangesAsync();

            return new PontoRespostaDTO
            {
                Sucesso           = true,
                Mensagem          = agora < inicioProva
                    ? $"Entrada registrada! O cálculo iniciará às {inicioProva:HH:mm} (horário oficial da prova)."
                    : "Entrada registrada! O cálculo iniciou agora.",
                HoraRegistrada    = agora.ToString("HH:mm"),
                HoraInicioCalculo = horaInicioCalculo.ToString("HH:mm"),
            };
        }

        // ── SAÍDA ─────────────────────────────────────────────────────────
        public async Task<PontoRespostaDTO> RegistrarSaidaAsync(
            int idUsuario, int idEvento, string tokenQRCode)
        {
            // 1. Valida o QR Code
            var tokenValido = await ValidarTokenAsync(idEvento, tokenQRCode);
            if (!tokenValido)
                return Erro("QR Code inválido ou expirado. Solicite um novo à coordenação.");

            // 2. Busca a alocação com check-in já feito
            var alocacao = await _context.Alocacoes
                .Include(a => a.IdEventoNavigation)
                .FirstOrDefaultAsync(a =>
                    a.IdUsuario == idUsuario &&
                    a.IdEvento  == idEvento  &&
                    a.StatusParticipacao == "Presente" &&
                    a.CheckInTime != null);

            if (alocacao == null)
                return Erro("Nenhuma entrada registrada para este evento.");

            if (alocacao.CheckOutTime != null)
                return Erro("Saída já registrada anteriormente.");

            var evento      = alocacao.IdEventoNavigation;
            var agora       = DateTime.UtcNow;

            // 3. Regra da janela mínima (70% da prova)
            //    Prova de 4h → saída liberada após 2h48
            var duracaoTotal   = evento.HorarioFim - evento.DataProva;
            var minimoPermitido = evento.DataProva.Add(duracaoTotal * 0.70);

            if (agora < minimoPermitido)
                return Erro($"Saída liberada somente após {minimoPermitido:HH:mm} " +
                            $"(70% da duração da prova).");

            // 4. Calcula horas trabalhadas
            //    Usa horaInicioCalculo (horário oficial ou chegada, o que for maior)
            var horaInicioCalculo = alocacao.CheckInTime!.Value < evento.DataProva
                ? evento.DataProva
                : alocacao.CheckInTime.Value;

            var horasTrabalhadas = (agora - horaInicioCalculo).TotalHours;

            // 5. Registra saída (StatusParticipacao já é "Presente" desde o check-in)
            alocacao.CheckOutTime     = agora;
            alocacao.HorasTrabalhadas = horasTrabalhadas;

            await _context.SaveChangesAsync();

            return new PontoRespostaDTO
            {
                Sucesso          = true,
                Mensagem         = "Saída registrada com sucesso!",
                HoraRegistrada   = agora.ToString("HH:mm"),
                HorasTrabalhadas = Math.Round(horasTrabalhadas, 2),
            };
        }

        // ── GERAR QR CODE (Admin) ─────────────────────────────────────────
        public async Task<QRCodeRespostaDTO> GerarQRCodeEventoAsync(int idEvento)
        {
            var evento = await _context.EventosProvas.FindAsync(idEvento);
            if (evento == null)
                throw new Exception("Evento não encontrado.");

            // Token único: idEvento + data + segredo fixo (hash SHA256)
            var segredo  = ObterSegredo();
            var base64   = $"{idEvento}:{ParaDataBrasilia(evento.DataProva):yyyyMMdd}:{segredo}";
            var bytes    = System.Text.Encoding.UTF8.GetBytes(base64);
            var hash     = System.Security.Cryptography.SHA256.HashData(bytes);
            var token    = Convert.ToHexString(hash)[..16]; // primeiros 16 caracteres

            return new QRCodeRespostaDTO
            {
                Token     = token,
                QRCodeUrl = $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data={token}",
                ExpiraEm  = $"Válido para {evento.DataProva:dd/MM/yyyy} — expira à meia-noite",
            };
        }

        // ── VALIDAR TOKEN ─────────────────────────────────────────────────
        public async Task<bool> ValidarTokenAsync(int idEvento, string token)
        {
            var evento = await _context.EventosProvas.FindAsync(idEvento);
            if (evento == null) return false;

            // Token só vale no dia do evento (dia de Brasília, não UTC)
            if (ParaDataBrasilia(evento.DataProva).Date != ParaDataBrasilia(DateTime.UtcNow).Date) return false;

            // Recalcula o token esperado
            var segredo  = ObterSegredo();
            var base64   = $"{idEvento}:{ParaDataBrasilia(evento.DataProva):yyyyMMdd}:{segredo}";
            var bytes    = System.Text.Encoding.UTF8.GetBytes(base64);
            var hash     = System.Security.Cryptography.SHA256.HashData(bytes);
            var tokenEsperado = Convert.ToHexString(hash)[..16];

            return token == tokenEsperado;
        }

       public async Task<List<HistoricoDTO>> ObterHistoricoAsync(int idUsuario)
         {
          var historico = await _context.Alocacoes
        .Include(a => a.IdEventoNavigation)
        .Where(a => a.IdUsuario == idUsuario && a.CheckOutTime != null) // Só provas já concluídas
        .Select(a => new HistoricoDTO
        {
            Id = a.IdAlocacao,
            Titulo = a.IdEventoNavigation.TituloProva,
            Serie = "Ensino Geral", 
            Data = a.IdEventoNavigation.DataProva.ToString("dd 'de' MMMM"),
            Entrada = a.CheckInTime.HasValue ? a.CheckInTime.Value.ToString("HH:mm") : "--:--",
            Saida = a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("HH:mm") : "--:--",
            HorasTrabalhadas = a.HorasTrabalhadas, 
            Funcao = a.PapelEvento,
            Status = "Concluída"
        })
        .ToListAsync();

      return historico;
      }
        // ── Helper ────────────────────────────────────────────────────────
        private static PontoRespostaDTO Erro(string mensagem) =>
            new() { Sucesso = false, Mensagem = mensagem };

        private string ObterSegredo() =>
            _configuration["Ponto:QrSecret"]
            ?? throw new InvalidOperationException("Configuração 'Ponto:QrSecret' não foi definida.");

        private static DateTime ParaDataBrasilia(DateTime utc)
        {
            var fusoBrasil = TimeZoneInfo.FindSystemTimeZoneById(
                Environment.OSVersion.Platform == PlatformID.Unix ? "America/Sao_Paulo" : "E. South America Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(utc, fusoBrasil);
        }
    }
}