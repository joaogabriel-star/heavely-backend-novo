// Services/Interfaces/IPontoService.cs
using SistemaHEAVELYBackend.DTOs.Ponto;

namespace SistemaHEAVELYBackend.Services.Interfaces
{
    public interface IPontoService
    {
        // Candidato registra entrada
        Task<PontoRespostaDTO> RegistrarEntradaAsync(int idUsuario, int idEvento);

        // Candidato registra saída (exige token do QR Code)
        Task<PontoRespostaDTO> RegistrarSaidaAsync(int idUsuario, int idEvento, string tokenQRCode);

        // Admin gera o QR Code do evento
        Task<QRCodeRespostaDTO> GerarQRCodeEventoAsync(int idEvento);

        // Valida se o token QR Code é válido para aquele evento
        Task<bool> ValidarTokenAsync(int idEvento, string token);

        Task<List<HistoricoDTO>> ObterHistoricoAsync(int idUsuario);
    }
}