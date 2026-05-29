namespace SistemaHEAVELYBackend.Services;

using SistemaHEAVELYBackend.DTOs.Usuarios;

public class UsuarioService : IUsuarioService
{
    private readonly AppDbContext _context;

    public UsuarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PerfilRespostaDTO> BuscarPerfilAsync(int idUsuario)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.IdPerfilNavigation)
            .Include(u => u.DadosAcademico)
            .Include(u => u.DadosAdministrativoIdUsuarioNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        return MontarPerfilResposta(usuario);
    }

    public async Task<PerfilRespostaDTO> AtualizarPerfilAsync(int idUsuario, AtualizarPerfilDTO dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.IdPerfilNavigation)
            .Include(u => u.DadosAcademico)
            .Include(u => u.DadosAdministrativoIdUsuarioNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        // Verifica se celular já pertence a outro usuário
        if (dto.Celular != null)
        {
            var celularEmUso = await _context.Usuarios.AnyAsync(u =>
                u.Celular == dto.Celular && u.IdUsuario != idUsuario);

            if (celularEmUso)
                throw new Exception("Este celular já está em uso.");

            usuario.Celular = dto.Celular;
        }

        // Atualiza só os campos preenchidos
        if (dto.NomeCompleto != null) usuario.NomeCompleto = dto.NomeCompleto;
        if (dto.Endereco != null) usuario.Endereco = dto.Endereco;

        // Atualiza dados acadêmicos se existirem
        if (usuario.DadosAcademico != null)
        {
            if (dto.EscolaridadeNivel != null)
                usuario.DadosAcademico.EscolaridadeNivel = dto.EscolaridadeNivel;
            if (dto.EscolaridadeStatus != null)
                usuario.DadosAcademico.EscolaridadeStatus = dto.EscolaridadeStatus;
            if (dto.CursoFormacao != null)
                usuario.DadosAcademico.CursoFormacao = dto.CursoFormacao;
            if (dto.ExperienciaProfissional != null)
                usuario.DadosAcademico.ExperienciaProfissional = dto.ExperienciaProfissional;
            if (dto.NivelIngles != null)
                usuario.DadosAcademico.NivelIngles = dto.NivelIngles;
            if (dto.NivelEspanhol != null)
                usuario.DadosAcademico.NivelEspanhol = dto.NivelEspanhol;
        }

        await _context.SaveChangesAsync();

        return MontarPerfilResposta(usuario);
    }

    public async Task AlterarSenhaAsync(int idUsuario, AlterarSenhaDTO dto)
    {
        var usuario = await _context.Usuarios.FindAsync(idUsuario);

        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        // Verifica se a senha atual está correta antes de trocar
        var senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.SenhaAtual, usuario.SenhaHash);
        if (!senhaCorreta)
            throw new Exception("Senha atual incorreta.");

        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);
        await _context.SaveChangesAsync();
    }

    public async Task<List<ListaUsuariosDTO>> ListarUsuariosAsync()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.IdPerfilNavigation)
            .Include(u => u.DadosAdministrativoIdUsuarioNavigation)
            .OrderBy(u => u.NomeCompleto)
            .ToListAsync();

        return usuarios.Select(MontarListaDTO).ToList();
    }

    public async Task<List<ListaUsuariosDTO>> ListarAdminsPendentesAsync()
    {
        
        var pendentes = await _context.Usuarios
            .Include(u => u.IdPerfilNavigation)
            .Include(u => u.DadosAcademico)
            //.Include(u => u.DadosAdministrativoIdUsuarioNavigation)// 
            .Where(u => u.StatusConta == "pendente") 
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();

        return pendentes.Select(MontarListaDTO).ToList();
    }

    

    public async Task AprovarAdminAsync(int idAdminAprovador, int idAdminAlvo, bool aprovar)
    {
        // Quem aprova não pode aprovar a si mesmo
        if (idAdminAprovador == idAdminAlvo)
            throw new Exception("Você não pode aprovar a si mesmo.");

        var usuario = await _context.Usuarios
            .Include(u => u.DadosAdministrativoIdUsuarioNavigation)
            .FirstOrDefaultAsync(u => u.IdUsuario == idAdminAlvo);

        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        if (usuario.StatusConta != "pendente")
            throw new Exception("Este usuário já foi processado.");

        if (aprovar)
        {
            // Aprova — ativa a conta e registra quem aprovou
            usuario.StatusConta = "ativo";

            if (usuario.DadosAdministrativoIdUsuarioNavigation != null)
            {
                usuario.DadosAdministrativoIdUsuarioNavigation.StatusAprovacao = "aprovado";
                usuario.DadosAdministrativoIdUsuarioNavigation.AprovadoPor = idAdminAprovador;
            }
        }
        else
        {
            // Rejeita
            usuario.StatusConta = "inativo";

            if (usuario.DadosAdministrativoIdUsuarioNavigation != null)
                usuario.DadosAdministrativoIdUsuarioNavigation.StatusAprovacao = "rejeitado";
        }

        await _context.SaveChangesAsync();
    }

    public async Task AlterarStatusContaAsync(int idUsuario, string novoStatus)
    {
        var statusValidos = new[] { "ativo", "inativo" };
        if (!statusValidos.Contains(novoStatus))
            throw new Exception("Status inválido. Use 'ativo' ou 'inativo'.");

        var usuario = await _context.Usuarios.FindAsync(idUsuario);

        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        usuario.StatusConta = novoStatus;
        await _context.SaveChangesAsync();
    }

    public async Task<List<ListaUsuariosDTO>> ListarMembrosAtivosAsync()
{
    return await _context.Usuarios
        .Include(u => u.IdPerfilNavigation)
        .Where(u => u.StatusConta == "ativo"
                     && u.IdPerfil != 3 
                     && u.IdPerfilNavigation.NomePerfil != "Admin" 
                     && u.IdPerfilNavigation.NomePerfil != "Administrador")
        .Select(u => new ListaUsuariosDTO
        {
            IdUsuario    = u.IdUsuario,
            NomeCompleto = u.NomeCompleto,
            Email        = u.Email,
            Cpf          = u.Cpf,
            Celular      = u.Celular,
            Perfil       = u.IdPerfilNavigation.NomePerfil,
            StatusConta  = u.StatusConta ?? string.Empty,
        })
        .ToListAsync();
}

public async Task<bool> AlterarPerfilMembroAsync(int idUsuario, string novoPerfil)
{
    var usuario = await _context.Usuarios.FindAsync(idUsuario);
    if (usuario == null) return false;

    // Busca o id do perfil pelo nome
    var perfil = await _context.Perfis
        .FirstOrDefaultAsync(p => p.NomePerfil == novoPerfil);
    if (perfil == null) return false;

    usuario.IdPerfil = perfil.IdPerfil;
    await _context.SaveChangesAsync();
    return true;
}
public async Task DeletarUsuarioAsync(int idUsuario)
{
    // 1. Procura e remove as alocações (inscrições em provas) do utilizador
    var alocacoes = await _context.Alocacoes.Where(a => a.IdUsuario == idUsuario).ToListAsync();
    if (alocacoes.Any())
    {
        _context.Alocacoes.RemoveRange(alocacoes);
    }

    // 2. Procura e remove os Dados Académicos do utilizador
    // Nota: Certifique-se de que o nome da tabela no seu _context é exatamente "DadosAcademicos"
    var dadosAcademicos = await _context.DadosAcademicos.Where(d => d.IdUsuario == idUsuario).ToListAsync();
    if (dadosAcademicos.Any())
    {
        _context.DadosAcademicos.RemoveRange(dadosAcademicos);
    }

    // 3. Se o sistema tiver tabelas de "DadosBancarios" ou "DadosAdministrativos", 
    // adicione o mesmo bloco para elas aqui antes de apagar o utilizador.

    // 4. Agora que os "filhos" foram limpos, procuramos o utilizador ("pai")
    var usuario = await _context.Usuarios.FindAsync(idUsuario);
    if (usuario == null)
    {
        throw new Exception("Usuário não encontrado no banco de dados.");
    }

    // 5. Remove o utilizador do sistema
    _context.Usuarios.Remove(usuario);
    
    // 6. Salva todas as alterações de uma vez só no banco de dados
    await _context.SaveChangesAsync();
}
    

    // ─── Métodos privados ─────────────────────────────────────────────────────

    private PerfilRespostaDTO MontarPerfilResposta(Usuario usuario)
    {
        var dto = new PerfilRespostaDTO
        {
            IdUsuario = usuario.IdUsuario,
            NomeCompleto = usuario.NomeCompleto,
            Cpf = usuario.Cpf,
            Celular = usuario.Celular,
            Email = usuario.Email,
            Endereco = usuario.Endereco,
            FotoPerfilUrl = usuario.FotoPerfilUrl,
            DataNascimento = usuario.DataNascimento.ToDateTime(TimeOnly.MinValue),
            StatusConta = usuario.StatusConta ?? string.Empty,
            Perfil = usuario.IdPerfilNavigation.NomePerfil
        };

        // Adiciona dados acadêmicos se existirem
        if (usuario.DadosAcademico != null)
        {
            dto.DadosAcademicos = new DadosAcademicoDTO
            {
                EscolaridadeNivel = usuario.DadosAcademico.EscolaridadeNivel,
                EscolaridadeStatus = usuario.DadosAcademico.EscolaridadeStatus,
                CursoFormacao = usuario.DadosAcademico.CursoFormacao,
                ExperienciaProfissional = usuario.DadosAcademico.ExperienciaProfissional,
                NivelIngles = usuario.DadosAcademico.NivelIngles,
                NivelEspanhol = usuario.DadosAcademico.NivelEspanhol,
                LinkDiplomaLedor = usuario.DadosAcademico.LinkDiplomaLedor,
                LinkNadaConsta = usuario.DadosAcademico.LinkNadaConsta
            };
        }

        // Adiciona dados administrativos se existirem
        if (usuario.DadosAdministrativoIdUsuarioNavigation != null)
        {
            dto.DadosAdministrativos = new DadosAdministrativoDTO
            {
                CargoInstituicao = usuario.DadosAdministrativoIdUsuarioNavigation.CargoInstituicao,
                EmailInstitucional = usuario.DadosAdministrativoIdUsuarioNavigation.EmailInstitucional,
                StatusAprovacao = usuario.DadosAdministrativoIdUsuarioNavigation.StatusAprovacao
            };
        }

        return dto;
    }

    private ListaUsuariosDTO MontarListaDTO(Usuario usuario)
    {
        var dadosAcademico = usuario.DadosAcademico;

        return new ListaUsuariosDTO
        {
            IdUsuario = usuario.IdUsuario,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            Celular = usuario.Celular,
            Perfil = usuario.IdPerfilNavigation.NomePerfil,
            StatusConta = usuario.StatusConta ?? string.Empty,
            CreatedAt = usuario.CreatedAt ?? DateTime.Now,
            StatusAprovacao = usuario.DadosAdministrativoIdUsuarioNavigation?.StatusAprovacao,
            CargoInstituicao = usuario.DadosAdministrativoIdUsuarioNavigation?.CargoInstituicao,
            Cpf = usuario.Cpf,
            DataNascimento = usuario.DataNascimento.ToString("yyyy-MM-dd"),
            Escolaridade = dadosAcademico?.EscolaridadeNivel ?? string.Empty,
            CursoFormacao = dadosAcademico?.CursoFormacao ?? string.Empty,
            MateriasFacilidade = dadosAcademico?.MateriasFacilidade ?? "Não informado", 
            LinkDiplomaLedor = dadosAcademico?.LinkDiplomaLedor
        };
    }
}