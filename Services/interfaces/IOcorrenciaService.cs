namespace SistemaHEAVELYBackend.Services.Interfaces;

public interface IOcorrenciaService
{
    Task<OcorrenciaRespostaDTO> CriarAsync(int idEvento, int idUsuario, CriarOcorrenciaDTO dto);
    Task<List<OcorrenciaRespostaDTO>> ListarAsync(int? idEvento);
}
