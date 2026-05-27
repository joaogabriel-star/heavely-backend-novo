namespace SistemaHEAVELYBackend.Services.Interfaces;

public interface IEventoService
{
    // Cria um novo evento — só Admin pode chamar isso
    Task<EventoRespostaDTO> CriarEventoAsync(CriarEventoDTO dto, int idAdmin);

    // Lista todos os eventos ativos — qualquer usuário logado vê
    Task<List<EventoRespostaDTO>> ListarEventosAsync();

    // Detalhes de um evento específico
    Task<EventoRespostaDTO> BuscarEventoPorIdAsync(int idEvento);

    // Edita um evento existente — só Admin
    Task<EventoRespostaDTO> AtualizarEventoAsync(int idEvento, AtualizarEventoDTO dto);

    // Cancela um evento — só Admin
    Task CancelarEventoAsync(int idEvento);

    // Adicione esta linha junto com as outras
    Task CandidatarSeAsync(int idEvento, int idUsuario, string papelEvento);
}