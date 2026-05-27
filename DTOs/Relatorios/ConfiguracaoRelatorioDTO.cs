namespace SistemaHEAVELYBackend.DTOs.Relatorios;

public class ConfiguracaoRelatorioDTO
{
    // Tipo de pagamento: "PorHora" ou "ValorFixo"
    [Required(ErrorMessage = "Tipo de pagamento é obrigatório")]
    public string TipoPagamento { get; set; } = string.Empty;

    // Valor por hora trabalhada (usado se TipoPagamento = "PorHora")
    public decimal ValorPorHora { get; set; }

    // Valor fixo independente das horas (usado se TipoPagamento = "ValorFixo")
    public decimal ValorFixo { get; set; }
}