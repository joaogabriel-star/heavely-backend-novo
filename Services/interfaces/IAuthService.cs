namespace SistemaHEAVELYBackend.Services.Interfaces;

public interface IAuthService
{
    Task<AuthRespostaDTO> CadastrarLedorFiscalAsync(CadastroLedorFiscalDTO dto);
    Task<AuthRespostaDTO> CadastrarAdminAsync(CadastroAdminDTO dto);
    Task<AuthRespostaDTO> LoginAsync(LoginDTO dto);
}