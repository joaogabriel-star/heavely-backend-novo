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

    [HttpPut("evento/{idEvento}/cancelar")]
[Authorize]
public async Task<IActionResult> CancelarPorCandidato(int idEvento, [FromBody] CancelarCandidatoDTO dto)
{
    try
    {
        // 1. Verifica se o motivo foi enviado corretamente
        if (dto == null || string.IsNullOrWhiteSpace(dto.Motivo))
            return BadRequest(new { mensagem = "O motivo do cancelamento não foi enviado ao servidor." });

        // 2. Busca o ID do usuário
        var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // 3. Busca a inscrição
        var alocacao = await _context.Alocacoes
            .FirstOrDefaultAsync(a => a.IdEvento == idEvento && a.IdUsuario == idUsuario);

        if (alocacao == null) 
            return NotFound(new { mensagem = "Inscrição não encontrada no banco de dados." });

        var papelCancelado = alocacao.PapelEvento;
        var eraConfirmado = alocacao.StatusParticipacao == "Confirmado";

        // 4. Cancela e salva o motivo
        alocacao.StatusParticipacao = "Cancelado";
        alocacao.Observacoes = $"Cancelado pelo candidato. Motivo: {dto.Motivo}";

        // 5. Puxa o próximo da reserva OU devolve a vaga
        if (eraConfirmado)
        {
            var proximo = await _context.Alocacoes
                // CORREÇÃO AQUI: Procurar quem está "Na Reserva" e não "Cancelado"
                .Where(a => a.IdEvento == idEvento && a.PapelEvento == papelCancelado && a.StatusParticipacao == "Na Reserva")
                .OrderBy(a => a.IdAlocacao)
                .FirstOrDefaultAsync();

            if (proximo != null)
            {
                // Se tem reserva, o próximo assume e a vaga continua ocupada
                proximo.StatusParticipacao = "Confirmado";
            }
            else
            {
                // Devolvemos a vaga para a Prova!
                var evento = await _context.EventosProvas.FindAsync(idEvento);
                
                if (evento != null)
                {
                    if (papelCancelado == "Ledor") 
                    {
                        evento.VagasLedor++; 
                    }
                    else if (papelCancelado == "Fiscal") 
                    {
                        evento.VagasFiscal++; 
                    }
                }
            }
        }

        // 6. Salva todas as mudanças de uma vez só!
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Inscrição cancelada com sucesso." });
    }
    catch (Exception ex)
    {
        return BadRequest(new { mensagem = $"Erro ao cancelar: {ex.Message}" });
    }
}
    // Classe auxiliar DTO
    public class AtualizarSalaDTO
    {
        public int IdAlocacao { get; set; }
        public string Sala { get; set; } = string.Empty;
    }

    public class CancelarCandidatoDTO
{
    public string Motivo { get; set; } = string.Empty;
}
}