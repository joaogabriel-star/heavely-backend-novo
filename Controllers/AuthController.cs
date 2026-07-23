namespace SistemaHEAVELYBackend.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("cadastro")]
    public async Task<IActionResult> CadastrarLedorFiscal([FromBody] CadastroLedorFiscalDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var resultado = await _authService.CadastrarLedorFiscalAsync(dto);
            return Created("", resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPost("cadastro-admin")]
    public async Task<IActionResult> CadastrarAdmin([FromBody] CadastroAdminDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var resultado = await _authService.CadastrarAdminAsync(dto);
            return Accepted(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPost("cadastro/{idUsuario}/documentos")]
    public async Task<IActionResult> EnviarDocumentos(int idUsuario, [FromForm] EnviarDocumentosDTO dto)
    {
        try
        {
            var resultado = await _authService.EnviarDocumentosAsync(idUsuario, dto);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var resultado = await _authService.LoginAsync(dto);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { mensagem = ex.Message });
        }
    }

    // POST /api/auth/esqueci-senha — sempre responde a mesma mensagem genérica,
    // exista ou não o email, e mesmo se o envio do email falhar internamente
    // (só loga no servidor) — não pode virar um jeito de descobrir quais
    // emails estão cadastrados.
    [HttpPost("esqueci-senha")]
    public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _authService.EsqueciSenhaAsync(dto);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[EsqueciSenha] Falha interna: {ex.Message}");
        }

        return Ok(new { mensagem = "Se o email existir em nosso sistema, enviamos um link de recuperação." });
    }

    // GET /api/auth/validar-token-reset/{token} — checagem leve pra tela de
    // redefinir senha decidir se mostra o formulário ou "link inválido/expirado".
    [HttpGet("validar-token-reset/{token}")]
    public async Task<IActionResult> ValidarTokenReset(string token)
    {
        var resultado = await _authService.ValidarTokenResetAsync(token);
        return Ok(resultado);
    }

    [HttpPost("redefinir-senha")]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _authService.RedefinirSenhaAsync(dto);
            return Ok(new { mensagem = "Senha redefinida com sucesso." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}