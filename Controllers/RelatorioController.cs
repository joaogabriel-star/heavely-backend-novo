namespace SistemaHEAVELYBackend.Controllers;

using Microsoft.AspNetCore.Authorization;
using SistemaHEAVELYBackend.DTOs.Relatorios;
using SistemaHEAVELYBackend.Services.Interfaces;

[ApiController]
[Route("api/relatorios")]
[Authorize(Roles = "Admin")] // só Admin acessa relatórios
public class RelatorioController : ControllerBase
{
    private readonly IRelatorioService _relatorioService;

    public RelatorioController(IRelatorioService relatorioService)
    {
        _relatorioService = relatorioService;
    }

    // GET /api/relatorios/{idEvento}/dados
    // Retorna os dados em JSON — útil para o frontend mostrar preview
    [HttpGet("{idEvento}/dados")]
    public async Task<IActionResult> DadosRelatorio(
        int idEvento, [FromQuery] ConfiguracaoRelatorioDTO config)
    {
        try
        {
            var dados = await _relatorioService.GerarDadosRelatorioAsync(idEvento, config);
            return Ok(dados);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // GET /api/relatorios/{idEvento}/pdf
    // Retorna o PDF para download direto
    [HttpGet("{idEvento}/pdf")]
    public async Task<IActionResult> DownloadPdf(
        int idEvento, [FromQuery] ConfiguracaoRelatorioDTO config)
    {
        try
        {
            var pdfBytes = await _relatorioService.GerarPdfRelatorioAsync(idEvento, config);

            // Monta o nome do arquivo com a data atual
            var nomeArquivo = $"relatorio_pagamento_{idEvento}_{DateTime.Now:yyyyMMdd}.pdf";

            // Retorna o arquivo para download
            return File(pdfBytes, "application/pdf", nomeArquivo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}