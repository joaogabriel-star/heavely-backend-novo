namespace SistemaHEAVELYBackend.Services.Interfaces;

using SistemaHEAVELYBackend.DTOs.Alocacoes;

public interface IAlocacaoService
{
    // Usuário se inscreve num evento
    Task<AlocacaoRespostaDTO> InscrevernAsync(int idEvento, int idUsuario, InscricaoDTO dto);

    // Usuário cancela a própria inscrição
    Task CancelarInscricaoAsync(int idEvento, int idUsuario);

    Task CancelarInscricaoAsync(int idAlocacao);

    // Promove o próximo da fila de reserva (por ordem de inscrição) pra Confirmado
    Task PromoverProximoDaReserva(int idEvento, string papelEvento);

    // Registra chegada no dia da prova
    Task<AlocacaoRespostaDTO> RegistrarCheckInAsync(int idEvento, int idUsuario);

    // Registra saída — fecha o cálculo de horas
    Task<AlocacaoRespostaDTO> RegistrarCheckOutAsync(int idEvento, int idUsuario);

    // Admin lista todos os inscritos de um evento
    Task<List<ListaInscritosDTO>> ListarInscritosAsync(int idEvento);

    // Usuário vê suas próprias inscrições
    Task<List<AlocacaoRespostaDTO>> ListarMinhasInscricoesAsync(int idUsuario);
}