namespace SistemaHEAVELYBackend.Services.Interfaces;

using SistemaHEAVELYBackend.DTOs.Relatorios;

public interface IRelatorioService
{
    // Retorna os dados do relatório em JSON (para o frontend visualizar)
    Task<RelatorioEventoDTO> GerarDadosRelatorioAsync(
        int idEvento, ConfiguracaoRelatorioDTO config);

    // Gera e retorna o PDF como array de bytes
    Task<byte[]> GerarPdfRelatorioAsync(
        int idEvento, ConfiguracaoRelatorioDTO config);
}