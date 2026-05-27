namespace SistemaHEAVELYBackend.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaHEAVELYBackend.DTOs.Alocacoes;
using SistemaHEAVELYBackend.Services.Interfaces;
using SistemaHEAVELYBackend.Data; // Certifique-se de que este using aponta para onde está o seu AppDbContext
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

[ApiController]
[Route("api/alocacoes")]
[Authorize]
public class AlocacaoController : ControllerBase
{
    private readonly IAlocacaoService _alocacaoService;
    private readonly AppDbContext _context;

    public AlocacaoController(IAlocacaoService alocacaoService, AppDbContext context)
    {
        _alocacaoService = alocacaoService;
        _context = context;
    }

    // POST /api/alocacoes/{idEvento}/inscrever
    [HttpPost("{idEvento}/inscrever")]
    public async Task<IActionResult> Inscrever(int idEvento, [FromBody] InscricaoDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var resultado = await _alocacaoService.InscrevernAsync(idEvento, idUsuario, dto);
            return Created("", resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // A ÚNICA ROTA DELETE (Evita o erro de conflito no Swagger)
    // DELETE /api/alocacoes/{idAlocacao}
    [HttpDelete("{idAlocacao}")]
    public async Task<IActionResult> DeletarAlocacao(int idAlocacao)
    {
        try
        {
            await _alocacaoService.CancelarInscricaoAsync(idAlocacao);
            return Ok(new { mensagem = "Inscrição cancelada e vaga liberada!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // GET /api/alocacoes/{idEvento}/lista — só Admin/Coordenação
    [HttpGet("{idEvento}/lista")]
    [Authorize(Roles = "Admin,Coordenacao")]
    public async Task<IActionResult> ListarInscritos(int idEvento)
    {
        try
        {
            var lista = await _alocacaoService.ListarInscritosAsync(idEvento);
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // GET /api/alocacoes/minhas — usuário vê suas próprias inscrições
    [HttpGet("minhas")]
    public async Task<IActionResult> MinhasInscricoes()
    {
        try
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var lista = await _alocacaoService.ListarMinhasInscricoesAsync(idUsuario);
            return Ok(lista);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // PUT /api/alocacoes/{idEvento}/checkin
    [HttpPut("{idEvento}/checkin")]
    public async Task<IActionResult> RegistrarCheckIn(int idEvento)
    {
        try
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var resultado = await _alocacaoService.RegistrarCheckInAsync(idEvento, idUsuario);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // PUT /api/alocacoes/{idEvento}/checkout
    [HttpPut("{idEvento}/checkout")]
    public async Task<IActionResult> RegistrarCheckOut(int idEvento)
    {
        try
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var resultado = await _alocacaoService.RegistrarCheckOutAsync(idEvento, idUsuario);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // PUT /api/alocacoes/salvar-salas
    [HttpPut("salvar-salas")]
    [Authorize(Roles = "Admin,Coordenacao")]
    public async Task<IActionResult> SalvarSalas([FromBody] List<AtualizarSalaDTO> salas)
    {
        try
        {
            foreach (var item in salas)
            {
                var alocacao = await _context.Alocacoes.FindAsync(item.IdAlocacao);
                if (alocacao != null)
                {
                    alocacao.SalaDesignada = item.Sala;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { mensagem = "Salas salvas com sucesso!" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // Classe auxiliar DTO
    public class AtualizarSalaDTO
    {
        public int IdAlocacao { get; set; }
        public string Sala { get; set; } = string.Empty;
    }
}