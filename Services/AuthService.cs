namespace SistemaHEAVELYBackend.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SistemaHEAVELYBackend.DTOs.Usuarios; // Ajuste se a sua pasta de DTOs for diferente
using SistemaHEAVELYBackend.Models;
using SistemaHEAVELYBackend.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Security.Cryptography;


public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly Cloudinary _cloudinary;
    private readonly ISendGridClient _sendGridClient;

    public AuthService(AppDbContext context, IConfiguration configuration, Cloudinary cloudinary, ISendGridClient sendGridClient)
    {
        _context = context;
        _configuration = configuration;
        _cloudinary = cloudinary;
        _sendGridClient = sendGridClient;
    }

    public async Task<AuthRespostaDTO> CadastrarLedorFiscalAsync(CadastroLedorFiscalDTO dto)
    {
        // Verifica se CPF, email ou celular já existem no banco
        var jaExiste = await _context.Usuarios.AnyAsync(u =>
            u.Cpf == dto.Cpf ||
            u.Email == dto.Email ||
            u.Celular == dto.Celular);

        if (jaExiste)
            throw new Exception("CPF, email ou celular já cadastrado.");

        // Verifica idade mínima de 18 anos
        var hoje = DateTime.Today;
        var idade = hoje.Year - dto.DataNascimento.Year;
        if (dto.DataNascimento.Date > hoje.AddYears(-idade)) idade--;
        if (idade < 18)
            throw new Exception("É necessário ter pelo menos 18 anos.");

        // Regra central: tem certificado = Ledor (1), não tem = Fiscal (2)
        var idPerfil = dto.PossuiCertificadoLedor ? 1 : 2;

        // Nunca salva senha em texto puro — sempre criptografada
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

        var usuario = new Usuario
        {
            IdPerfil = idPerfil,
            NomeCompleto = dto.NomeCompleto,
            Cpf = dto.Cpf,
            Celular = dto.Celular,
            Email = dto.Email,
            SenhaHash = senhaHash,
            Endereco = dto.Endereco,
            DataNascimento = DateOnly.FromDateTime(dto.DataNascimento),
            StatusConta = "pendente", 
            CreatedAt = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Cria os dados acadêmicos vinculados ao usuário recém-criado
        var dadosAcademicos = new DadosAcademico
        {
            IdUsuario = usuario.IdUsuario,
            EscolaridadeNivel = dto.EscolaridadeNivel,
            EscolaridadeStatus = dto.EscolaridadeStatus,
            CursoFormacao = dto.InstituicaoEnsino, 
            MateriasFacilidade = dto.MateriasFacilidade,
            NivelIngles = dto.NivelIngles,
            NivelEspanhol = dto.NivelEspanhol,
            ExperienciaProfissional = dto.ExperienciaProfissional
        };

        _context.DadosAcademicos.Add(dadosAcademicos);
        await _context.SaveChangesAsync();

        var nomePerfil = dto.PossuiCertificadoLedor ? "Ledor" : "Fiscal";

        return new AuthRespostaDTO
        {
            IdUsuario = usuario.IdUsuario,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            Perfil = nomePerfil,
            StatusConta = usuario.StatusConta,
            Token = string.Empty // 👈 NINGUÉM ENTRA SEM APROVAÇÃO DO COORDENADOR!
        };
    }

    public async Task<AuthRespostaDTO> CadastrarAdminAsync(CadastroAdminDTO dto)
    {
        var jaExiste = await _context.Usuarios.AnyAsync(u =>
            u.Cpf == dto.Cpf ||
            u.Email == dto.Email);

        if (jaExiste)
            throw new Exception("CPF ou email já cadastrado.");

        var senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

        var usuario = new Usuario
        {
            IdPerfil = 3, // Admin
            NomeCompleto = dto.NomeCompleto,
            Cpf = dto.Cpf,
            Celular = dto.Celular,
            Email = dto.Email,
            SenhaHash = senhaHash,
            DataNascimento = DateOnly.FromDateTime(dto.DataNascimento),
            StatusConta = "pendente", // Fica pendente até aprovação
            CreatedAt = DateTime.Now
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var dadosAdmin = new DadosAdministrativo
        {
            IdUsuario = usuario.IdUsuario,
            CargoInstituicao = dto.CargoInstituicao,
            EmailInstitucional = dto.EmailInstitucional,
            StatusAprovacao = "pendente"
        };

        _context.DadosAdministrativos.Add(dadosAdmin);
        await _context.SaveChangesAsync();

        // Admin pendente não recebe token ainda
        return new AuthRespostaDTO
        {
            IdUsuario = usuario.IdUsuario,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            Perfil = "Admin",
            StatusConta = "pendente",
            Token = string.Empty
        };
    }

    public async Task<AuthRespostaDTO> LoginAsync(LoginDTO dto)
    {
        // Busca o usuário pelo email já trazendo o Perfil (JOIN automático)
        var usuario = await _context.Usuarios
            .Include(u => u.IdPerfilNavigation)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);

        // Mensagem genérica — não revela se é email ou senha que está errado
        if (usuario == null)
            throw new Exception("Email ou senha inválidos.");

        var senhaCorreta = BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash);
        if (!senhaCorreta)
            throw new Exception("Email ou senha inválidos.");

        // Bloqueia contas pendentes ou inativas
        if (usuario.StatusConta != "ativo")
            throw new Exception($"Conta com status '{usuario.StatusConta}'. Aguarde aprovação ou contate a coordenação.");

        var nomePerfil = usuario.IdPerfilNavigation.NomePerfil;
        var token = GerarToken(usuario, nomePerfil);

        return new AuthRespostaDTO
        {
            IdUsuario = usuario.IdUsuario,
            NomeCompleto = usuario.NomeCompleto,
            Email = usuario.Email,
            Perfil = nomePerfil,
            StatusConta = usuario.StatusConta,
            Token = token
        };
    }

    // Método privado — só usado internamente pelo Service
    private string GerarToken(Usuario usuario, string nomePerfil)
    {
        // Claims são as informações gravadas dentro do token
        // O frontend consegue ler sem consultar o banco
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, nomePerfil),
            new Claim(ClaimTypes.Name, usuario.NomeCompleto)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiracao = int.Parse(_configuration["Jwt:ExpiracaoHoras"]!);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expiracao),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // ── UPLOAD DE DOCUMENTOS (diploma / nada consta) ──────────────────────
    public async Task<EnviarDocumentosRespostaDTO> EnviarDocumentosAsync(int idUsuario, EnviarDocumentosDTO dto)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.DadosAcademico)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            throw new Exception("Usuário não encontrado.");

        if (usuario.DadosAcademico == null)
            throw new Exception("Este usuário não possui um cadastro de Ledor/Fiscal válido para receber documentos.");

        if (usuario.StatusConta != "pendente")
            throw new Exception("Só é possível enviar documentos enquanto o cadastro está pendente de aprovação.");

        if (dto.Diploma == null && dto.NadaConsta == null)
            throw new Exception("Envie ao menos um arquivo (diploma ou nada consta).");

        const long tamanhoMaximo = 5 * 1024 * 1024; // 5MB
        var tiposPermitidos = new[] { "application/pdf", "image/jpeg", "image/png" };

        void ValidarArquivo(IFormFile? arquivo, string nomeCampo)
        {
            if (arquivo == null) return;
            if (arquivo.Length > tamanhoMaximo)
                throw new Exception($"O arquivo '{nomeCampo}' excede o limite de 5MB.");
            if (!tiposPermitidos.Contains(arquivo.ContentType))
                throw new Exception($"O arquivo '{nomeCampo}' precisa ser PDF, JPG ou PNG.");
        }

        var resposta = new EnviarDocumentosRespostaDTO();

        // Validação de tamanho/tipo e o "já enviado" são POR CAMPO, dentro do
        // try/catch de cada arquivo — um arquivo inválido/já enviado não pode
        // impedir o outro (válido) de ser processado e salvo.
        if (dto.Diploma != null)
        {
            try
            {
                ValidarArquivo(dto.Diploma, "diploma");
                if (usuario.DadosAcademico.LinkDiplomaLedor != null)
                    throw new Exception("O diploma já foi enviado para este cadastro.");

                usuario.DadosAcademico.LinkDiplomaLedor = await UploadArquivoAsync(dto.Diploma);
                resposta.DiplomaEnviado = true;
            }
            catch (Exception ex)
            {
                resposta.ErroDiploma = ex.Message;
            }
        }

        if (dto.NadaConsta != null)
        {
            try
            {
                ValidarArquivo(dto.NadaConsta, "nada consta");
                if (usuario.DadosAcademico.LinkNadaConsta != null)
                    throw new Exception("O nada consta já foi enviado para este cadastro.");

                usuario.DadosAcademico.LinkNadaConsta = await UploadArquivoAsync(dto.NadaConsta);
                resposta.NadaConstaEnviado = true;
            }
            catch (Exception ex)
            {
                resposta.ErroNadaConsta = ex.Message;
            }
        }

        await _context.SaveChangesAsync();

        return resposta;
    }

    // ── ESQUECI MINHA SENHA ─────────────────────────────────────────────────
    public async Task EsqueciSenhaAsync(EsqueciSenhaDTO dto)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email);

        // Se o email não existir, não faz nada — quem chama (Controller) sempre
        // devolve a mesma mensagem genérica, então isso não é observável de fora.
        if (usuario == null)
            return;

        // Invalida qualquer token anterior ainda não usado — só o link mais
        // recente deve continuar funcionando.
        var tokensAnteriores = await _context.SenhaResetTokens
            .Where(t => t.IdUsuario == usuario.IdUsuario && t.UsadoEm == null)
            .ToListAsync();
        foreach (var antigo in tokensAnteriores)
            antigo.UsadoEm = DateTime.UtcNow;

        var tokenCru = GerarTokenAleatorio();

        _context.SenhaResetTokens.Add(new SenhaResetToken
        {
            IdUsuario = usuario.IdUsuario,
            TokenHash = CalcularHashToken(tokenCru),
            ExpiraEm = DateTime.UtcNow.AddHours(1),
            CriadoEm = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        // O token em texto puro só existe aqui e no link do email — nunca é salvo.
        await EnviarEmailResetAsync(usuario, tokenCru);
    }

    public async Task<ValidarTokenResetRespostaDTO> ValidarTokenResetAsync(string token)
    {
        var tokenHash = CalcularHashToken(token);
        var registro = await _context.SenhaResetTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        var valido = registro != null
            && registro.UsadoEm == null
            && registro.ExpiraEm > DateTime.UtcNow;

        return new ValidarTokenResetRespostaDTO { Valido = valido };
    }

    public async Task RedefinirSenhaAsync(RedefinirSenhaDTO dto)
    {
        // Revalida tudo de novo no servidor — a checagem da tela (ValidarTokenResetAsync)
        // é só UX, essa aqui é a que realmente decide se a senha troca.
        var tokenHash = CalcularHashToken(dto.Token);
        var registro = await _context.SenhaResetTokens
            .Include(t => t.IdUsuarioNavigation)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (registro == null || registro.UsadoEm != null || registro.ExpiraEm <= DateTime.UtcNow)
            throw new Exception("Link inválido ou expirado. Solicite um novo.");

        registro.IdUsuarioNavigation.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.NovaSenha);
        registro.UsadoEm = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static string GerarTokenAleatorio() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)); // 64 chars hex — só [0-9A-F], seguro em URL

    private static string CalcularHashToken(string tokenCru) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(tokenCru)));

    private async Task EnviarEmailResetAsync(Usuario usuario, string tokenCru)
    {
        var frontendUrl = _configuration["App:FrontendUrl"]
            ?? throw new InvalidOperationException("Configuração 'App:FrontendUrl' não foi definida.");
        var fromEmail = _configuration["SendGrid:FromEmail"]
            ?? throw new InvalidOperationException("Configuração 'SendGrid:FromEmail' não foi definida.");
        var fromName = _configuration["SendGrid:FromName"] ?? "Heavely";

        var link = $"{frontendUrl.TrimEnd('/')}/redefinir-senha/{tokenCru}";

        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(usuario.Email, usuario.NomeCompleto);
        var mensagem = MailHelper.CreateSingleEmail(
            from, to,
            "Recuperação de senha — Heavely",
            $"Recebemos um pedido de redefinição de senha para sua conta Heavely.\n\n" +
            $"Acesse o link abaixo pra definir uma nova senha (válido por 1 hora):\n{link}\n\n" +
            $"Se você não pediu isso, pode ignorar este email.",
            $"<p>Recebemos um pedido de redefinição de senha para sua conta Heavely.</p>" +
            $"<p><a href=\"{link}\">Clique aqui para definir uma nova senha</a> (link válido por 1 hora).</p>" +
            $"<p>Se você não pediu isso, pode ignorar este email.</p>"
        );

        var resposta = await _sendGridClient.SendEmailAsync(mensagem);
        if ((int)resposta.StatusCode >= 400)
        {
            var corpo = await resposta.Body.ReadAsStringAsync();
            throw new Exception($"Falha ao enviar email de recuperação (SendGrid {resposta.StatusCode}): {corpo}");
        }
    }

    private async Task<string> UploadArquivoAsync(IFormFile arquivo)
    {
        await using var stream = arquivo.OpenReadStream();
        var fileDescription = new FileDescription(arquivo.FileName, stream);

        UploadResult uploadResult = arquivo.ContentType == "application/pdf"
            ? await _cloudinary.UploadAsync(new RawUploadParams { File = fileDescription })
            : await _cloudinary.UploadAsync(new ImageUploadParams { File = fileDescription });

        if (uploadResult.Error != null)
            throw new Exception(uploadResult.Error.Message);

        return uploadResult.SecureUrl.ToString();
    }
}