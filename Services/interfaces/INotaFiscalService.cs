namespace SistemaHEAVELYBackend.Services.Interfaces;

using SistemaHEAVELYBackend.DTOs.NotaFiscal;

public interface INotaFiscalService
{
    // Retorna os dados agregados da Nota Fiscal em JSON (para o frontend visualizar)
    Task<NotaFiscalRespostaDTO> GerarDadosNotaFiscalAsync(NotaFiscalFiltroDTO filtro);

    // Gera e retorna o PDF como array de bytes
    Task<byte[]> GerarPdfNotaFiscalAsync(NotaFiscalFiltroDTO filtro);
}
