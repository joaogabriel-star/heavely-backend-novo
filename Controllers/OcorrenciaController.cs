namespace SistemaHEAVELYBackend.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaHEAVELYBackend.DTOs.Ocorrencias;
using SistemaHEAVELYBackend.Services.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api")]
[Authorize]
public class OcorrenciaController : ControllerBase
{
    private readonly IOcorrenciaService _ocorrenciaService;

    public OcorrenciaController(IOcorrenciaService ocorrenciaService)
    {
        _ocorrenciaService = ocorrenciaService;
    }

    // POST /api/eventos/{idEvento}/ocorrencias — qualquer usuário logado relata
    [HttpPost("eventos/{idEvento}/ocorrencias")]
    public async Task<IActionResult> Criar(int idEvento, [FromBody] CriarOcorrenciaDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var resultado = await _ocorrenciaService.CriarAsync(idEvento, idUsuario, dto);
            return Created("", resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // GET /api/ocorrencias?idEvento= — só Admin/Coordenacao
    [HttpGet("ocorrencias")]
    [Authorize(Roles = "Admin,Coordenacao")]
    public async Task<IActionResult> Listar([FromQuery] int? idEvento)
    {
        try
        {
            var resultado = await _ocorrenciaService.ListarAsync(idEvento);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
