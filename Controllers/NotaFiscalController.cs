namespace SistemaHEAVELYBackend.Controllers;

using Microsoft.AspNetCore.Authorization;
using SistemaHEAVELYBackend.DTOs.NotaFiscal;
using SistemaHEAVELYBackend.Services.Interfaces;

[ApiController]
[Route("api/nota-fiscal")]
[Authorize(Roles = "Admin")] // só Admin acessa a Nota Fiscal agregada
public class NotaFiscalController : ControllerBase
{
    private readonly INotaFiscalService _notaFiscalService;

    public NotaFiscalController(INotaFiscalService notaFiscalService)
    {
        _notaFiscalService = notaFiscalService;
    }

    // GET /api/nota-fiscal/dados
    // Retorna os dados agregados em JSON — útil para o frontend mostrar preview
    [HttpGet("dados")]
    public async Task<IActionResult> DadosNotaFiscal([FromQuery] NotaFiscalFiltroDTO filtro)
    {
        try
        {
            var dados = await _notaFiscalService.GerarDadosNotaFiscalAsync(filtro);
            return Ok(dados);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    // GET /api/nota-fiscal/pdf
    // Retorna o PDF para download direto
    [HttpGet("pdf")]
    public async Task<IActionResult> DownloadPdf([FromQuery] NotaFiscalFiltroDTO filtro)
    {
        try
        {
            var pdfBytes = await _notaFiscalService.GerarPdfNotaFiscalAsync(filtro);
            var nomeArquivo = $"nota_fiscal_{filtro.DataInicio:yyyyMMdd}_{filtro.DataFim:yyyyMMdd}.pdf";
            return File(pdfBytes, "application/pdf", nomeArquivo);
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
