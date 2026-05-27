namespace SistemaHEAVELYBackend.Controllers;

using Microsoft.AspNetCore.Authorization;
using SistemaHEAVELYBackend.DTOs.Usuarios;
using SistemaHEAVELYBackend.Services.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuarioController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    // GET /api/usuarios/perfil — usuário vê o próprio perfil
    [HttpGet("perfil")]
    public async Task<IActionResult> BuscarPerfil()
    {
        try
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var perfil = await _usuarioService.BuscarPerfilAsync(idUsuario);
            return Ok(perfil);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // PUT /api/usuarios/perfil — usuário atualiza os próprios dados
    [HttpPut("perfil")]
    public async Task<IActionResult> AtualizarPerfil([FromBody] AtualizarPerfilDTO dto)
    {
        try
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var perfil = await _usuarioService.AtualizarPerfilAsync(idUsuario, dto);
            return Ok(perfil);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // PUT /api/usuarios/senha — usuário altera a própria senha
    [HttpPut("senha")]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _usuarioService.AlterarSenhaAsync(idUsuario, dto);
            return Ok(new { mensagem = "Senha alterada com sucesso." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // GET /api/usuarios — lista todos (só Admin)
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ListarUsuarios()
    {
        try
        {
            var lista = await _usuarioService.ListarUsuariosAsync();
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // GET /api/usuarios/pendentes — admins pendentes (só Admin)
    [HttpGet("pendentes")]
    public async Task<IActionResult> ListarPendentes()
    {
        try
        {
            var lista = await _usuarioService.ListarAdminsPendentesAsync();
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    [HttpGet("ativos")]
    public async Task<IActionResult> ListarMembrosAtivos()
    {
        try
        {
            var lista = await _usuarioService.ListarMembrosAtivosAsync();
            return Ok(lista);
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }


    [HttpPut("{id}/perfil")]
    public async Task<IActionResult> AlterarPerfil(int id, [FromQuery] string perfil)
    {
        try
        {
            await _usuarioService.AlterarPerfilMembroAsync(id, perfil);
            return Ok(new { mensagem = "Função alterada com sucesso!" });
        }
        catch (Exception ex) { return BadRequest(new { mensagem = ex.Message }); }
    }
    // PUT /api/usuarios/{id}/aprovar — aprova admin (só Admin)
    [HttpPut("{id}/aprovar")]
    public async Task<IActionResult> AprovarAdmin(int id, [FromQuery] bool aprovar = true)
    {
        try
        {
            var idAprovador = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _usuarioService.AprovarAdminAsync(idAprovador, id, aprovar);

            var msg = aprovar ? "Admin aprovado com sucesso." : "Admin rejeitado.";
            return Ok(new { mensagem = msg });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // PUT /api/usuarios/{id}/status — ativa ou inativa usuário (só Admin)
    [HttpPut("{id}/status")]
    public async Task<IActionResult> AlterarStatus(int id, [FromQuery] string status)
    {
        try
        {
            await _usuarioService.AlterarStatusContaAsync(id, status);
            return Ok(new { mensagem = $"Status alterado para '{status}' com sucesso." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}