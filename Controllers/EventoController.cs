namespace SistemaHEAVELYBackend.Controllers;

using Microsoft.AspNetCore.Authorization;
using SistemaHEAVELYBackend.DTOs.Eventos;
using SistemaHEAVELYBackend.Services.Interfaces;
using System.Security.Claims;

[ApiController]
[Route("api/eventos")]
[Authorize] // qualquer endpoint aqui exige login (token JWT)
public class EventoController : ControllerBase
{
    private readonly IEventoService _eventoService;

    public EventoController(IEventoService eventoService)
    {
        _eventoService = eventoService;
    }

    // GET /api/eventos — lista todos (qualquer usuário logado)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ListarEventos()
    {
        try
        {
            var eventos = await _eventoService.ListarEventosAsync();
            return Ok(eventos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // GET /api/eventos/5 — detalhes de um evento
    [HttpGet("{id}")]
    public async Task<IActionResult> BuscarEvento(int id)
    {
        try
        {
            var evento = await _eventoService.BuscarEventoPorIdAsync(id);
            return Ok(evento);
        }
        catch (Exception ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    // POST /api/eventos — cria evento (só Admin)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CriarEvento([FromBody] CriarEventoDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Pega o id do admin logado direto do token JWT
            var idAdmin = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var evento = await _eventoService.CriarEventoAsync(dto, idAdmin);
            return Created("", evento);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // POST /api/eventos/{id}/candidatar
    [HttpPost("{id}/candidatar")]
[Authorize]
public async Task<IActionResult> Candidatar(int id, [FromBody] CandidaturaDTO dto)
{
    try
    {
        var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _eventoService.CandidatarSeAsync(id, idUsuario, dto.PapelEvento);
        
        return Ok(new { mensagem = "Candidatura realizada com sucesso!" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = ex.Message });
    }
}

    // PUT /api/eventos/5 — edita evento (só Admin)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AtualizarEvento(int id, [FromBody] AtualizarEventoDTO dto)
    {
        try
        {
            var evento = await _eventoService.AtualizarEventoAsync(id, dto);
            return Ok(evento);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // DELETE /api/eventos/5 — cancela evento (só Admin)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CancelarEvento(int id)
    {
        try
        {
            await _eventoService.CancelarEventoAsync(id);
            return Ok(new { mensagem = "Evento cancelado com sucesso." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
public class CandidaturaDTO
{
    public string PapelEvento { get; set; } = string.Empty;
}
    
}