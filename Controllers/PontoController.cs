// Controllers/PontoController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SistemaHEAVELYBackend.DTOs.Ponto;
using SistemaHEAVELYBackend.Services.Interfaces;

namespace SistemaHEAVELYBackend.Controllers
{
    [ApiController]
    [Route("api/ponto")]
    [Authorize]
    public class PontoController : ControllerBase
    {
        private readonly IPontoService _pontoService;

        public PontoController(IPontoService pontoService)
        {
            _pontoService = pontoService;
        }

        // Pega o ID do usuário logado direto do token JWT
        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // POST /api/ponto/entrada
        [HttpPost("entrada")]
        public async Task<IActionResult> Entrada([FromBody] RegistrarPontoDTO dto)
        {
            var resultado = await _pontoService.RegistrarEntradaAsync(GetUserId(), dto.IdEvento);
            return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
        }

        // POST /api/ponto/saida
        [HttpPost("saida")]
        public async Task<IActionResult> Saida([FromBody] RegistrarPontoDTO dto)
        {
            if (string.IsNullOrEmpty(dto.TokenQRCode))
                return BadRequest(new { mensagem = "Token do QR Code é obrigatório." });

            var resultado = await _pontoService
                .RegistrarSaidaAsync(GetUserId(), dto.IdEvento, dto.TokenQRCode);

            return resultado.Sucesso ? Ok(resultado) : BadRequest(resultado);
        }

        // GET /api/ponto/qrcode/{idEvento} — só Admin
        [HttpGet("qrcode/{idEvento}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GerarQRCode(int idEvento)
        {
            var resultado = await _pontoService.GerarQRCodeEventoAsync(idEvento);
            return Ok(resultado);
        }

        [HttpGet("historico")]
       public async Task<IActionResult> GetHistorico()
      {
    // Descobre quem é o usuário logado
      var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Pede ao Service para buscar no banco
      var historico = await _pontoService.ObterHistoricoAsync(idUsuario);

    // Devolve para o React!
      return Ok(historico);
       }
    }
}