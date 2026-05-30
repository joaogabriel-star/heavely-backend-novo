namespace SistemaHEAVELYBackend.Services;

using Microsoft.EntityFrameworkCore;
using SistemaHEAVELYBackend.DTOs.Alocacoes;
using SistemaHEAVELYBackend.Models;
using SistemaHEAVELYBackend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class AlocacaoService : IAlocacaoService
{
    private readonly AppDbContext _context;

    public AlocacaoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AlocacaoRespostaDTO> InscrevernAsync(
        int idEvento, int idUsuario, InscricaoDTO dto)
    {
        // ── Validação 1: evento existe e está ativo ──────────────────────────
        var evento = await _context.EventosProvas
            .Include(e => e.Alocacos)
            .FirstOrDefaultAsync(e => e.IdEvento == idEvento);

        if (evento == null)
            throw new Exception("Evento não encontrado.");

        if (evento.StatusEvento == "CANCELADO")
            throw new Exception("Este evento foi cancelado.");

        if (evento.StatusEvento == "ENCERRADO")
            throw new Exception("Este evento já foi encerrado.");

        if (evento.DataProva < DateTime.Now)
            throw new Exception("Não é possível se inscrever em um evento que já ocorreu.");

        // ── Validação 2: papel válido ─────────────────────────────────────────
        var papelValido = dto.PapelEvento == "Ledor" || dto.PapelEvento == "Fiscal";
        if (!papelValido)
            throw new Exception("Papel inválido. Use 'Ledor' ou 'Fiscal'.");

        // ── Validação 3: usuário tem perfil compatível com o papel ────────────
        var usuario = await _context.Usuarios
            .Include(u => u.IdPerfilNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        // Fiscal não pode se inscrever como Ledor
        if (dto.PapelEvento == "Ledor" && usuario.IdPerfilNavigation.NomePerfil == "Fiscal")
            throw new Exception("Fiscais não podem se inscrever como Ledor.");

        // ── Validação 4: usuário já inscrito nesse evento ─────────────────────
        var jaInscrito = await _context.Alocacoes.AnyAsync(a =>
            a.IdEvento == idEvento &&
            a.IdUsuario == idUsuario &&
            a.StatusParticipacao != "Cancelado");

        if (jaInscrito)
            throw new Exception("Você já está inscrito neste evento.");

        // ── Lógica de vagas ───────────────────────────────────────────────────
        // Conta quantos confirmados já existem para esse papel
        var confirmados = evento.Alocacos.Count(a =>
            a.PapelEvento == dto.PapelEvento &&
            a.StatusParticipacao == "Confirmado");

        var totalVagas = dto.PapelEvento == "Ledor"
            ? evento.VagasLedor
            : evento.VagasFiscal;

        string status;
        int? posicaoReserva = null;

        if (confirmados < totalVagas)
        {
            // Tem vaga — entra como Confirmado
            status = "Confirmado";
        }
        else
        {
            // Sem vaga — entra na fila de reserva
            // Descobre qual é a última posição na fila para esse papel
            var ultimaPosicao = await _context.Alocacoes
                .Where(a => a.IdEvento == idEvento &&
                            a.PapelEvento == dto.PapelEvento &&
                            a.StatusParticipacao == "Reserva")
                .MaxAsync(a => (int?)a.PosicaoReserva) ?? 0;

            status = "Reserva";
            posicaoReserva = ultimaPosicao + 1;
        }

        // ── Cria a alocação ───────────────────────────────────────────────────
        var alocacao = new Alocaco
        {
            IdEvento = idEvento,
            IdUsuario = idUsuario,
            PapelEvento = dto.PapelEvento,
            StatusParticipacao = status,
            PosicaoReserva = posicaoReserva,
            DataInscricao = DateTime.Now
        };

        _context.Alocacoes.Add(alocacao);
        await _context.SaveChangesAsync();

        alocacao.IdEventoNavigation = evento; // Associa para o MontarResposta conseguir ler as observações

        return MontarResposta(alocacao, evento.TituloProva, usuario.NomeCompleto);
    }

    // ── CANCELAMENTO PELO CANDIDATO (Mantém histórico como Cancelado) ──────
    public async Task CancelarInscricaoAsync(int idEvento, int idUsuario)
    {
        var alocacao = await _context.Alocacoes
            .FirstOrDefaultAsync(a =>
                a.IdEvento == idEvento &&
                a.IdUsuario == idUsuario &&
                a.StatusParticipacao != "Cancelado");

        if (alocacao == null)
            throw new Exception("Inscrição não encontrada.");

        var eraConfirmado = alocacao.StatusParticipacao == "Confirmado";
        var papelCancelado = alocacao.PapelEvento;

        // Atualiza para cancelado em vez de remover do banco
        alocacao.StatusParticipacao = "Cancelado";
        alocacao.PosicaoReserva = null;
        await _context.SaveChangesAsync();

        // ── Lógica de reserva ─────────────────────────────────────────────────
        // Se era confirmado, sobe o primeiro da fila de reserva
        if (eraConfirmado)
        {
            await PromoverProximoDaReserva(idEvento, papelCancelado);
        }
    }

    // ── CANCELAMENTO PELO COORDENADOR (Apaga do banco e puxa a fila) ──────
   // ── CANCELAMENTO PELO COORDENADOR (Apaga do banco e trata a vaga livre) ──────
public async Task CancelarInscricaoAsync(int idAlocacao)
{
    var alocacao = await _context.Alocacoes.FindAsync(idAlocacao);
    
    if (alocacao != null)
    {
        var idEvento = alocacao.IdEvento;
        var papelCancelado = alocacao.PapelEvento;
        var eraConfirmado = alocacao.StatusParticipacao == "Confirmado";

        // 1. Remove a inscrição definitivamente (Isto já liberta a vaga no cálculo dinâmico!)
        _context.Alocacoes.Remove(alocacao);
        await _context.SaveChangesAsync();

        // 2. Verifica se precisamos de puxar alguém da fila para ocupar a vaga que acabou de abrir
        if (eraConfirmado)
        {
            var proximoDaReserva = await _context.Alocacoes
                .Where(a => a.IdEvento == idEvento && 
                            a.PapelEvento == papelCancelado && 
                            a.StatusParticipacao == "Na Reserva")
                .OrderBy(a => a.IdAlocacao) // Garante que pega o primeiro que entrou na fila
                .FirstOrDefaultAsync();

            if (proximoDaReserva != null)
            {
                // Promovemos a pessoa da reserva
                proximoDaReserva.StatusParticipacao = "Confirmado";
                await _context.SaveChangesAsync();
            }
            // Se a fila estiver vazia, não fazemos nada. A vaga já está livre porque a alocação anterior foi apagada!
        }
    }
}

    // ── MÉTODO REUTILIZÁVEL PARA PUXAR A FILA ─────────────────────────────
    private async Task PromoverProximoDaReserva(int idEvento, string papelEvento)
    {
        var proximoDaReserva = await _context.Alocacoes
            .Where(a => a.IdEvento == idEvento &&
                        a.PapelEvento == papelEvento &&
                        a.StatusParticipacao == "Reserva")
            .OrderBy(a => a.PosicaoReserva) // Pega o 1º da fila
            .FirstOrDefaultAsync();

        if (proximoDaReserva != null)
        {
            // Promove o reserva para Confirmado
            proximoDaReserva.StatusParticipacao = "Confirmado";
            proximoDaReserva.PosicaoReserva = null; // Saiu da reserva

            // Reordena a fila — todos descem uma posição
            var restantesNaReserva = await _context.Alocacoes
                .Where(a => a.IdEvento == idEvento &&
                            a.PapelEvento == papelEvento &&
                            a.StatusParticipacao == "Reserva")
                .OrderBy(a => a.PosicaoReserva)
                .ToListAsync();

            for (int i = 0; i < restantesNaReserva.Count; i++)
            {
                restantesNaReserva[i].PosicaoReserva = i + 1;
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task<AlocacaoRespostaDTO> RegistrarCheckInAsync(int idEvento, int idUsuario)
    {
        var alocacao = await _context.Alocacoes
            .FirstOrDefaultAsync(a =>
                a.IdEvento == idEvento &&
                a.IdUsuario == idUsuario &&
                a.StatusParticipacao == "Confirmado");

        if (alocacao == null)
            throw new Exception("Inscrição confirmada não encontrada para este evento.");

        if (alocacao.CheckInTime.HasValue)
            throw new Exception("Check-in já foi realizado.");

        alocacao.CheckInTime = DateTime.Now;
        alocacao.StatusParticipacao = "Presente";
        await _context.SaveChangesAsync();

        var evento = await _context.EventosProvas.FindAsync(idEvento);
        var usuario = await _context.Usuarios.FindAsync(idUsuario);

        return MontarResposta(alocacao, evento!.TituloProva, usuario!.NomeCompleto);
    }

    public async Task<AlocacaoRespostaDTO> RegistrarCheckOutAsync(int idEvento, int idUsuario)
    {
        var alocacao = await _context.Alocacoes
            .Include(a => a.IdEventoNavigation) 
            .FirstOrDefaultAsync(a =>
                a.IdEvento == idEvento &&
                a.IdUsuario == idUsuario &&
                a.StatusParticipacao == "Presente");

        if (alocacao == null)
            throw new Exception("Check-in não foi realizado ainda.");

        if (alocacao.CheckOutTime.HasValue)
            throw new Exception("Check-out já foi realizado.");

        alocacao.CheckOutTime = DateTime.Now;
        await _context.SaveChangesAsync();

        var usuario = await _context.Usuarios.FindAsync(idUsuario);

        return MontarResposta(alocacao, alocacao.IdEventoNavigation!.TituloProva, usuario!.NomeCompleto);
    }

    public async Task<List<ListaInscritosDTO>> ListarInscritosAsync(int idEvento)
    {
        return await _context.Alocacoes
            .Include(a => a.IdUsuarioNavigation)
            .Where(a => a.IdEvento == idEvento)
            .Select(a => new ListaInscritosDTO
            {
                IdAlocacao         = a.IdAlocacao,
                NomeUsuario        = a.IdUsuarioNavigation!.NomeCompleto,
                Email              = a.IdUsuarioNavigation.Email,
                PapelEvento        = a.PapelEvento,
                StatusParticipacao = a.StatusParticipacao,
                PosicaoReserva     = a.PosicaoReserva,
                CheckInTime        = a.CheckInTime,
                CheckOutTime       = a.CheckOutTime,
                HorasTrabalhadas   = a.HorasTrabalhadas,
            })
            .ToListAsync();
    }

    public async Task<List<AlocacaoRespostaDTO>> ListarMinhasInscricoesAsync(int idUsuario)
    {
        var inscricoes = await _context.Alocacoes
            .Include(a => a.IdEventoNavigation) // Inclui o Evento para ler as Observacoes
            .Where(a => a.IdUsuario == idUsuario &&
                        a.StatusParticipacao != "Cancelado") // Oculta as canceladas da tela do candidato
            .OrderByDescending(a => a.IdEventoNavigation.DataProva)
            .ToListAsync();

        return inscricoes.Select(a => MontarResposta(
            a,
            a.IdEventoNavigation?.TituloProva ?? "Sem título",
            string.Empty
        )).ToList();
    }

    // ─── Métodos privados ─────────────────────────────────────────────────────

    private double? CalcularHoras(DateTime? checkIn, DateTime? checkOut, DateTime dataOficialProva)
    {
        if (!checkIn.HasValue || !checkOut.HasValue)
            return null;

        var inicioRealCalculo = checkIn.Value > dataOficialProva ? dataOficialProva : checkIn.Value;

        if (checkOut.Value < inicioRealCalculo)
            return 0;

        return Math.Round((checkOut.Value - inicioRealCalculo).TotalHours, 2);
    }

    private AlocacaoRespostaDTO MontarResposta(
        Alocaco alocacao, string tituloProva, string nomeUsuario)
    {
        return new AlocacaoRespostaDTO
        {
            IdAlocacao = alocacao.IdAlocacao,
            IdEvento = alocacao.IdEvento,
            TituloProva = tituloProva,
            NomeUsuario = nomeUsuario,
            PapelEvento = alocacao.PapelEvento,
            StatusParticipacao = alocacao.StatusParticipacao,
            PosicaoReserva = alocacao.PosicaoReserva,
            DataInscricao = alocacao.DataInscricao ?? DateTime.Now,
            CheckInTime = alocacao.CheckInTime,
            CheckOutTime = alocacao.CheckOutTime,
            HorasTrabalhadas = CalcularHoras(alocacao.CheckInTime, alocacao.CheckOutTime, alocacao.IdEventoNavigation?.DataProva ?? DateTime.Now),
            SalaDesignada = alocacao.SalaDesignada,
            Observacoes = alocacao.IdEventoNavigation?.Observacoes
        };
    }
}