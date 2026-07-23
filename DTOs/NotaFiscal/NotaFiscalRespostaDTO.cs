namespace SistemaHEAVELYBackend.DTOs.NotaFiscal;

public class NotaFiscalRespostaDTO
{
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFim { get; set; }

    public List<SegmentoNotaFiscalDTO> Segmentos { get; set; } = new();

    // Quantos EVENTOS (não linhas de pessoa/dia) no período caíram em "Sem Série" — aviso pra tela
    public int EventosSemSerieClassificavel { get; set; }

    public double TotalGeralHoras { get; set; }
    public decimal TotalGeralValor { get; set; }
}

public class SegmentoNotaFiscalDTO
{
    // "EF2", "EM" ou "Sem Série / Não Classificado"
    public string NomeSegmento { get; set; } = string.Empty;

    public List<PessoaNotaFiscalDTO> Pessoas { get; set; } = new();

    public double TotalSegmentoHoras { get; set; }
    public decimal TotalSegmentoValor { get; set; }
}

public class PessoaNotaFiscalDTO
{
    public int IdUsuario { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;

    public List<DiaTrabalhadoDTO> Dias { get; set; } = new();

    public double TotalHoras { get; set; }
    public decimal TotalValor { get; set; }
}

public class DiaTrabalhadoDTO
{
    public DateTime Data { get; set; }
    public string TituloEvento { get; set; } = string.Empty;
    public string? Serie { get; set; }
    public string PapelEvento { get; set; } = string.Empty;
    public double? HorasTrabalhadas { get; set; }
    public decimal ValorDia { get; set; }

    // Fez check-in mas nunca teve check-out — Horas/Valor ficam zerados por
    // esse motivo (não por erro de dados). Usado só pra anotar a linha do relatório.
    public bool SaidaNaoRegistrada { get; set; }
}
