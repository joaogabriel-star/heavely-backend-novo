namespace SistemaHEAVELYBackend.Services.Interfaces;

public interface IAuthService
{
    Task<AuthRespostaDTO> CadastrarLedorFiscalAsync(CadastroLedorFiscalDTO dto);
    Task<AuthRespostaDTO> CadastrarAdminAsync(CadastroAdminDTO dto);
    Task<AuthRespostaDTO> LoginAsync(LoginDTO dto);
    Task<EnviarDocumentosRespostaDTO> EnviarDocumentosAsync(int idUsuario, EnviarDocumentosDTO dto);
    Task EsqueciSenhaAsync(EsqueciSenhaDTO dto);
    Task<ValidarTokenResetRespostaDTO> ValidarTokenResetAsync(string token);
    Task RedefinirSenhaAsync(RedefinirSenhaDTO dto);
}