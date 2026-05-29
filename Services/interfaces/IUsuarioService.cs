namespace SistemaHEAVELYBackend.Services.Interfaces;

using SistemaHEAVELYBackend.DTOs.Usuarios;

public interface IUsuarioService
{
    // Usuário vê o próprio perfil
    Task<PerfilRespostaDTO> BuscarPerfilAsync(int idUsuario);

    // Usuário atualiza os próprios dados
    Task<PerfilRespostaDTO> AtualizarPerfilAsync(int idUsuario, AtualizarPerfilDTO dto);

    // Usuário altera a própria senha
    Task AlterarSenhaAsync(int idUsuario, AlterarSenhaDTO dto);

    // Admin lista todos os usuários
    Task<List<ListaUsuariosDTO>> ListarUsuariosAsync();

    // Admin lista admins pendentes de aprovação
    Task<List<ListaUsuariosDTO>> ListarAdminsPendentesAsync();

    // Admin aprova ou rejeita um coordenador
    Task AprovarAdminAsync(int idAdminAprovador, int idAdminAlvo, bool aprovar);

    Task DeletarUsuarioAsync(int idUsuario);

    // Admin ativa ou inativa qualquer usuário
    Task AlterarStatusContaAsync(int idUsuario, string novoStatus);

    Task<List<ListaUsuariosDTO>> ListarMembrosAtivosAsync();
    Task<bool>                  AlterarPerfilMembroAsync(int idUsuario, string novoPerfil);
}