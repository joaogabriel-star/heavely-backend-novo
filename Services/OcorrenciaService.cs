namespace SistemaHEAVELYBackend.Services;

public class OcorrenciaService : IOcorrenciaService
{
    private readonly AppDbContext _context;

    public OcorrenciaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OcorrenciaRespostaDTO> CriarAsync(int idEvento, int idUsuario, CriarOcorrenciaDTO dto)
    {
        var evento = await _context.EventosProvas.FindAsync(idEvento);
        if (evento == null)
            throw new Exception("Evento não encontrado.");

        var usuario = await _context.Usuarios.FindAsync(idUsuario);
        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        var ocorrencia = new Ocorrencia
        {
            IdEvento = idEvento,
            IdUsuario = idUsuario,
            Tipo = dto.Tipo,
            Descricao = dto.Descricao,
            CreatedAt = DateTime.Now
        };

        _context.Ocorrencias.Add(ocorrencia);
        await _context.SaveChangesAsync();

        return new OcorrenciaRespostaDTO
        {
            IdOcorrencia = ocorrencia.IdOcorrencia,
            IdEvento = idEvento,
            TituloProva = evento.TituloProva,
            NomeUsuario = usuario.NomeCompleto,
            Tipo = ocorrencia.Tipo,
            Descricao = ocorrencia.Descricao,
            CreatedAt = ocorrencia.CreatedAt
        };
    }

    public async Task<List<OcorrenciaRespostaDTO>> ListarAsync(int? idEvento)
    {
        var query = _context.Ocorrencias
            .Include(o => o.IdEventoNavigation)
            .Include(o => o.IdUsuarioNavigation)
            .AsQueryable();

        if (idEvento.HasValue)
            query = query.Where(o => o.IdEvento == idEvento.Value);

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OcorrenciaRespostaDTO
            {
                IdOcorrencia = o.IdOcorrencia,
                IdEvento = o.IdEvento,
                TituloProva = o.IdEventoNavigation.TituloProva,
                NomeUsuario = o.IdUsuarioNavigation.NomeCompleto,
                Tipo = o.Tipo,
                Descricao = o.Descricao,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync();
    }
}
