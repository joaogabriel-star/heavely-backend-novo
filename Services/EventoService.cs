namespace SistemaHEAVELYBackend.Services;

using SistemaHEAVELYBackend.DTOs.Eventos;

public class EventoService : IEventoService
{
    private readonly AppDbContext _context;

    public EventoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EventoRespostaDTO> CriarEventoAsync(CriarEventoDTO dto, int idAdmin)
    {
        if (dto.HorarioFim <= dto.DataProva)
            throw new Exception("O horário de fim deve ser posterior ao horário de início.");

        var dataProvaUtc = ConverterHorarioBrasiliaParaUtc(dto.DataProva);
        var horarioFimUtc = ConverterHorarioBrasiliaParaUtc(dto.HorarioFim);

        if (dataProvaUtc < DateTime.UtcNow)
            throw new Exception("Não é possível criar um evento em uma data passada.");

        var evento = new EventosProva
        {
            IdCriadorAdmin = idAdmin,
            TituloProva = dto.TituloProva,
            LocalProva = dto.LocalProva,
            DataProva = dataProvaUtc,
            HorarioFim = horarioFimUtc,
            VagasLedor = dto.VagasLedor,
            VagasFiscal = dto.VagasFiscal,
            StatusEvento = "ATIVO",
            Serie = dto.Serie,
            ValorHora = dto.ValorHora,
            Observacoes = dto.Observacoes
        };

        _context.EventosProvas.Add(evento);        // ← corrigido
        await _context.SaveChangesAsync();

        var admin = await _context.Usuarios.FindAsync(idAdmin);

        return MontarResposta(evento, admin?.NomeCompleto ?? "Desconhecido", 0, 0);
    }

    public async Task<List<EventoRespostaDTO>> ListarEventosAsync()
    {
        var eventos = await _context.EventosProvas
            .Where(e => e.StatusEvento == "ATIVO")
            .OrderBy(e => e.DataProva)
            .ToListAsync();

        var listaRetorno = new List<EventoRespostaDTO>();

        foreach (var e in eventos)
        {
            var dto = MapearResposta(e);

            // 1. Conta apenas quem está Confirmado ou Presente
            var qtdLedor = await _context.Alocacoes
                .CountAsync(a => a.IdEvento == e.IdEvento 
                              && a.PapelEvento == "Ledor"
                              && (a.StatusParticipacao == "Confirmado" || a.StatusParticipacao == "Presente"));
                
            var qtdFiscal = await _context.Alocacoes
                .CountAsync(a => a.IdEvento == e.IdEvento 
                              && a.PapelEvento == "Fiscal"
                              && (a.StatusParticipacao == "Confirmado" || a.StatusParticipacao == "Presente"));

            // NOVO: Contar quantos estão na reserva para mostrar no frontend
            var qtdReserva = await _context.Alocacoes
                .CountAsync(a => a.IdEvento == e.IdEvento 
                              && a.StatusParticipacao == "Na Reserva");

            // 2. Faz a matemática e preenche o DTO
            dto.VagasLedorDisponiveis = e.VagasLedor - qtdLedor;
            dto.VagasFiscalDisponiveis = e.VagasFiscal - qtdFiscal;
            dto.VagasPreenchidas = qtdLedor + qtdFiscal;
            // Se o teu DTO não tiver a propriedade Reservas, terás de a adicionar no DTO, ou podes ignorar esta linha:
            // dto.Reservas = qtdReserva; 

            listaRetorno.Add(dto);
        }

        return listaRetorno;
    }

    public async Task<EventoRespostaDTO> BuscarEventoPorIdAsync(int idEvento)
    {
        var evento = await _context.EventosProvas   // ← corrigido
            .Include(e => e.IdCriadorAdminNavigation)
            .Include(e => e.Alocacos)               // ← corrigido
            .FirstOrDefaultAsync(e => e.IdEvento == idEvento);

        if (evento == null)
            throw new Exception("Evento não encontrado.");

        return MontarResposta(
            evento,
            evento.IdCriadorAdminNavigation.NomeCompleto,
            ContarVagasOcupadas(evento, "Ledor"),
            ContarVagasOcupadas(evento, "Fiscal")
        );
    }

public async Task<EventoRespostaDTO> AtualizarEventoAsync(int idEvento, AtualizarEventoDTO dto)
{
    var evento = await _context.EventosProvas 
        .Include(e => e.IdCriadorAdminNavigation)
        .Include(e => e.Alocacos) 
        .FirstOrDefaultAsync(e => e.IdEvento == idEvento);

    if (evento == null)
        throw new Exception("Evento não encontrado.");

    if (evento.StatusEvento == "CANCELADO")
        throw new Exception("Não é possível editar um evento cancelado.");

    if (dto.TituloProva != null) evento.TituloProva = dto.TituloProva;
    if (dto.LocalProva != null) evento.LocalProva = dto.LocalProva;

    if (dto.DataProva.HasValue)
        evento.DataProva = ConverterHorarioBrasiliaParaUtc(dto.DataProva.Value);

    if (dto.HorarioFim.HasValue)
        evento.HorarioFim = ConverterHorarioBrasiliaParaUtc(dto.HorarioFim.Value);

    if (dto.VagasLedor.HasValue) evento.VagasLedor = dto.VagasLedor.Value;
    if (dto.VagasFiscal.HasValue) evento.VagasFiscal = dto.VagasFiscal.Value;

    if (dto.Serie != null) evento.Serie = dto.Serie;
    if (dto.ValorHora.HasValue) evento.ValorHora = dto.ValorHora.Value;
    if (dto.Observacoes != null) evento.Observacoes = dto.Observacoes;

    if (evento.HorarioFim <= evento.DataProva)
        throw new Exception("O horário de fim deve ser posterior ao horário de início.");

    await _context.SaveChangesAsync();

    return MontarResposta(
        evento,
        evento.IdCriadorAdminNavigation.NomeCompleto,
        ContarVagasOcupadas(evento, "Ledor"),
        ContarVagasOcupadas(evento, "Fiscal")
    );
}

    public async Task CancelarEventoAsync(int idEvento)
    {
        var evento = await _context.EventosProvas   // ← corrigido
            .FirstOrDefaultAsync(e => e.IdEvento == idEvento);

        if (evento == null)
            throw new Exception("Evento não encontrado.");

        if (evento.StatusEvento == "CANCELADO")
            throw new Exception("Evento já está cancelado.");

        evento.StatusEvento = "CANCELADO";
        await _context.SaveChangesAsync();
    }

    // ─── Métodos privados ────────────────────────────────────────────────────

    // Delega para FusoHorarioHelper (compartilhado com NotaFiscalService) — não duplicar.
    private DateTime ConverterHorarioBrasiliaParaUtc(DateTime horarioBrasilia)
    {
        return FusoHorarioHelper.BrasiliaParaUtc(horarioBrasilia);
    }

    private int ContarVagasOcupadas(EventosProva evento, string papel)
    {
        return evento.Alocacos           // ← mantido como no teu código original
            .Count(a => a.PapelEvento == papel &&
                        (a.StatusParticipacao == "Confirmado" || a.StatusParticipacao == "Presente"));
    }

    private EventoRespostaDTO MontarResposta(
        EventosProva evento,
        string nomeAdmin,
        int ledoresConfirmados,
        int fiscaisConfirmados)
    {
        return new EventoRespostaDTO
        {
            IdEvento = evento.IdEvento,
            TituloProva = evento.TituloProva,
            LocalProva = evento.LocalProva ?? string.Empty,
            DataProva = evento.DataProva,
            HorarioFim = evento.HorarioFim,
            VagasLedor = evento.VagasLedor,
            VagasFiscal = evento.VagasFiscal,
            StatusEvento = evento.StatusEvento ?? "ATIVO",
            CriadoPor = nomeAdmin,
            Serie = evento.Serie,
            ValorHora = evento.ValorHora,
            Observacoes = evento.Observacoes,
            VagasLedorDisponiveis = evento.VagasLedor - ledoresConfirmados,
            VagasFiscalDisponiveis = evento.VagasFiscal - fiscaisConfirmados
        };
    }
    
    public Task<EventoRespostaDTO> BuscarEventoPorIdAsync(int idEvento, int idUsuario, string papelEvento)
    {
        throw new NotImplementedException();
    }

    private EventoRespostaDTO MapearResposta(EventosProva e)
{
    return new EventoRespostaDTO
    {
        IdEvento = e.IdEvento,
        TituloProva = e.TituloProva, // Use apenas o que existe no modelo
        DataProva = e.DataProva,
        HorarioFim = e.HorarioFim,
        VagasLedor = e.VagasLedor,
        VagasFiscal = e.VagasFiscal,
        StatusEvento = e.StatusEvento ?? "ATIVO",
        Serie = e.Serie,
        ValorHora = e.ValorHora,
        Observacoes = e.Observacoes
    };
}
}