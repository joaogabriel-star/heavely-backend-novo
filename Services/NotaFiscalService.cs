namespace SistemaHEAVELYBackend.Services;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaHEAVELYBackend.DTOs.NotaFiscal;

public class NotaFiscalService : INotaFiscalService
{
    // Valor por hora sempre fixo pra Nota Fiscal — nunca usa evento.ValorHora
    // (esse campo é exclusivo do relatório por-evento em RelatorioService).
    private const decimal ValorHoraNotaFiscal = 37m;

    private const string NomeSegmentoSemSerie = "Sem Série / Não Classificado";

    private readonly AppDbContext _context;

    public NotaFiscalService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<NotaFiscalRespostaDTO> GerarDadosNotaFiscalAsync(NotaFiscalFiltroDTO filtro)
    {
        var inicioUtc = FusoHorarioHelper.BrasiliaParaUtc(filtro.DataInicio.Date);
        var fimUtcExclusivo = FusoHorarioHelper.BrasiliaParaUtc(filtro.DataFim.Date.AddDays(1));

        if (fimUtcExclusivo <= inicioUtc)
            throw new Exception("Data de fim deve ser posterior à data de início.");

        var query = _context.Alocacoes
            .Include(a => a.IdEventoNavigation)
            .Include(a => a.IdUsuarioNavigation)
            .Where(a => a.IdEventoNavigation.DataProva >= inicioUtc
                     && a.IdEventoNavigation.DataProva < fimUtcExclusivo
                     && (a.StatusParticipacao == "Presente" || a.CheckInTime.HasValue));

        if (!string.IsNullOrWhiteSpace(filtro.Serie))
            query = query.Where(a => a.IdEventoNavigation.Serie == filtro.Serie);

        if (filtro.IdUsuario.HasValue)
            query = query.Where(a => a.IdUsuario == filtro.IdUsuario.Value);

        var linhas = await query.ToListAsync();

        // Filtro de Segmento roda em memória, reaproveitando a MESMA função usada
        // pro agrupamento — evita ter duas implementações da regra EF2/EM que podem divergir.
        var linhasNoEscopo = string.IsNullOrWhiteSpace(filtro.Segmento)
            ? linhas
            : linhas.Where(a => DerivarSegmento(a.IdEventoNavigation.Serie) == filtro.Segmento).ToList();

        var eventosSemSerie = linhasNoEscopo
            .Where(a => DerivarSegmento(a.IdEventoNavigation.Serie) == NomeSegmentoSemSerie)
            .Select(a => a.IdEvento)
            .Distinct()
            .Count();

        var segmentos = linhasNoEscopo
            .GroupBy(a => DerivarSegmento(a.IdEventoNavigation.Serie))
            .Select(MontarSegmento)
            .OrderBy(s => s.NomeSegmento == NomeSegmentoSemSerie ? 1 : 0) // "Sem Série" sempre por último
            .ThenBy(s => s.NomeSegmento)
            .ToList();

        return new NotaFiscalRespostaDTO
        {
            PeriodoInicio = filtro.DataInicio.Date,
            PeriodoFim = filtro.DataFim.Date,
            Segmentos = segmentos,
            EventosSemSerieClassificavel = eventosSemSerie,
            TotalGeralHoras = segmentos.Sum(s => s.TotalSegmentoHoras),
            TotalGeralValor = segmentos.Sum(s => s.TotalSegmentoValor)
        };
    }

    public async Task<byte[]> GerarPdfNotaFiscalAsync(NotaFiscalFiltroDTO filtro)
    {
        var dados = await GerarDadosNotaFiscalAsync(filtro);

        QuestPDF.Settings.License = LicenseType.Community;

        var totalPessoasUnicas = dados.Segmentos
            .SelectMany(s => s.Pessoas)
            .Select(p => p.IdUsuario)
            .Distinct()
            .Count();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                // ── Cabeçalho ────────────────────────────────────────────────
                page.Header().Column(col =>
                {
                    col.Item().Text("NOTA FISCAL — RELATÓRIO AGREGADO")
                        .FontSize(16).Bold().AlignCenter();

                    col.Item().PaddingTop(4).Text(
                        $"Período: {dados.PeriodoInicio:dd/MM/yyyy} a {dados.PeriodoFim:dd/MM/yyyy}")
                        .FontSize(13).SemiBold().AlignCenter();

                    var filtrosAplicados = new List<string>();
                    if (!string.IsNullOrWhiteSpace(filtro.Segmento)) filtrosAplicados.Add($"Segmento={filtro.Segmento}");
                    if (!string.IsNullOrWhiteSpace(filtro.Serie)) filtrosAplicados.Add($"Série={filtro.Serie}");
                    if (filtro.IdUsuario.HasValue) filtrosAplicados.Add($"Pessoa=ID {filtro.IdUsuario}");

                    if (filtrosAplicados.Count > 0)
                        col.Item().PaddingTop(2).Text($"Filtros aplicados: {string.Join(" | ", filtrosAplicados)}")
                            .FontSize(9).AlignCenter().FontColor(Colors.Grey.Darken1);

                    col.Item().PaddingTop(4).LineHorizontal(1);
                });

                // ── Conteúdo ─────────────────────────────────────────────────
                page.Content().PaddingVertical(10).Column(col =>
                {
                    if (dados.EventosSemSerieClassificavel > 0)
                    {
                        col.Item().PaddingBottom(10).Background("#FEF3C7").BorderColor("#FDE68A")
                            .Border(1).Padding(8)
                            .Text($"⚠ {dados.EventosSemSerieClassificavel} evento(s) no período não têm Série " +
                                  "classificável em EF2/EM — revisar manualmente (ver bloco \"Sem Série\" abaixo).")
                            .FontColor("#92400E").SemiBold();
                    }

                    foreach (var segmento in dados.Segmentos)
                    {
                        var ehSemSerie = segmento.NomeSegmento == NomeSegmentoSemSerie;
                        var corBarra = ehSemSerie ? "#F59E0B" : "#2C3E50";
                        var corSubtotal = ehSemSerie ? "#FEF3C7" : "#D5E8D4";
                        Color corTextoSubtotal = ehSemSerie ? "#92400E" : Colors.Black;

                        col.Item().PaddingTop(14).Background(corBarra).Padding(6)
                            .Text(segmento.NomeSegmento).FontColor(Colors.White).Bold().FontSize(12);

                        foreach (var pessoa in segmento.Pessoas)
                        {
                            col.Item().PaddingTop(8).Text(
                                $"{pessoa.NomeCompleto} — CPF {FormatarCpf(pessoa.Cpf)}")
                                .FontSize(10).SemiBold();

                            col.Item().PaddingTop(3).Table(tabela =>
                            {
                                tabela.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(2); // Data
                                    cols.RelativeColumn(4); // Evento
                                    cols.RelativeColumn(2); // Série
                                    cols.RelativeColumn(2); // Papel
                                    cols.RelativeColumn(2); // Horas
                                    cols.RelativeColumn(2); // Valor
                                });

                                tabela.Header(header =>
                                {
                                    var headerStyle = TextStyle.Default.FontSize(8).Bold();
                                    foreach (var titulo in new[] { "Data", "Evento", "Série", "Papel", "Horas", "Valor (R$)" })
                                        header.Cell().Background("#2C3E50").Padding(5)
                                            .Text(titulo).Style(headerStyle).FontColor(Colors.White);
                                });

                                for (int i = 0; i < pessoa.Dias.Count; i++)
                                {
                                    var dia = pessoa.Dias[i];
                                    var bg = i % 2 == 0 ? "#F8F9FA" : "#FFFFFF";

                                    tabela.Cell().Background(bg).Padding(4).Text($"{dia.Data:dd/MM/yyyy}");
                                    tabela.Cell().Background(bg).Padding(4).Text(dia.TituloEvento);
                                    tabela.Cell().Background(bg).Padding(4).Text(dia.Serie ?? "—");
                                    tabela.Cell().Background(bg).Padding(4).Text(dia.PapelEvento);
                                    tabela.Cell().Background(bg).Padding(4)
                                        .Text(dia.HorasTrabalhadas.HasValue ? $"{dia.HorasTrabalhadas:F1}h" : "-");
                                    tabela.Cell().Background(bg).Padding(4)
                                        .Text($"R$ {dia.ValorDia:F2}").AlignRight();
                                }

                                tabela.Cell().ColumnSpan(5).Background(corSubtotal).Padding(4)
                                    .Text($"Subtotal {pessoa.NomeCompleto}").Bold().FontColor(corTextoSubtotal);
                                tabela.Cell().Background(corSubtotal).Padding(4)
                                    .Text($"R$ {pessoa.TotalValor:F2}").Bold().AlignRight().FontColor(corTextoSubtotal);
                            });
                        }

                        col.Item().PaddingTop(6).Background(corBarra).Padding(6).Row(row =>
                        {
                            row.RelativeItem().Text($"SUBTOTAL {segmento.NomeSegmento}")
                                .FontColor(Colors.White).Bold();
                            row.RelativeItem().AlignRight().Text($"R$ {segmento.TotalSegmentoValor:F2}")
                                .FontColor(Colors.White).Bold();
                        });
                    }

                    // ── Totalizador geral ─────────────────────────────────────
                    col.Item().PaddingTop(16).Table(tabela =>
                    {
                        tabela.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(1);
                        });

                        tabela.Cell().Background("#2C3E50").Padding(6)
                            .Text("Total de Pessoas (únicas)").FontColor(Colors.White).Bold();
                        tabela.Cell().Background("#2C3E50").Padding(6)
                            .Text($"{totalPessoasUnicas}").FontColor(Colors.White).Bold().AlignRight();

                        tabela.Cell().Background("#34495E").Padding(6)
                            .Text("Total de Horas").FontColor(Colors.White).Bold();
                        tabela.Cell().Background("#34495E").Padding(6)
                            .Text($"{dados.TotalGeralHoras:F1}h").FontColor(Colors.White).Bold().AlignRight();

                        tabela.Cell().Background("#27AE60").Padding(6)
                            .Text("TOTAL GERAL A PAGAR (todos os blocos)").FontColor(Colors.White).Bold().FontSize(11);
                        tabela.Cell().Background("#27AE60").Padding(6)
                            .Text($"R$ {dados.TotalGeralValor:F2}").FontColor(Colors.White).Bold().FontSize(11).AlignRight();
                    });
                });

                // ── Rodapé ───────────────────────────────────────────────────
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Página ").FontSize(8);
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" de ").FontSize(8);
                        text.TotalPages().FontSize(8);
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }

    private static string FormatarCpf(string cpf)
    {
        if (cpf.Length != 11) return cpf;
        return $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";
    }

    private static SegmentoNotaFiscalDTO MontarSegmento(IGrouping<string, Alocaco> grupo)
    {
        var pessoas = grupo
            .GroupBy(a => a.IdUsuario)
            .Select(MontarPessoa)
            .OrderBy(p => p.NomeCompleto)
            .ToList();

        return new SegmentoNotaFiscalDTO
        {
            NomeSegmento = grupo.Key,
            Pessoas = pessoas,
            TotalSegmentoHoras = pessoas.Sum(p => p.TotalHoras),
            TotalSegmentoValor = pessoas.Sum(p => p.TotalValor)
        };
    }

    private static PessoaNotaFiscalDTO MontarPessoa(IGrouping<int, Alocaco> grupo)
    {
        var dias = grupo
            .OrderBy(a => a.IdEventoNavigation.DataProva)
            .Select(a =>
            {
                // Sempre recalcula a partir de CheckInTime/CheckOutTime — não confia na coluna
                // HorasTrabalhadas persistida, mesmo padrão do RelatorioService existente.
                var horas = CalculoHorasHelper.Calcular(
                    a.CheckInTime, a.CheckOutTime, a.IdEventoNavigation.DataProva);

                return new DiaTrabalhadoDTO
                {
                    Data = a.IdEventoNavigation.DataProva,
                    TituloEvento = a.IdEventoNavigation.TituloProva,
                    Serie = a.IdEventoNavigation.Serie,
                    PapelEvento = a.PapelEvento,
                    HorasTrabalhadas = horas,
                    ValorDia = Math.Round((decimal)(horas ?? 0) * ValorHoraNotaFiscal, 2)
                };
            })
            .ToList();

        var pessoa = grupo.First().IdUsuarioNavigation;

        return new PessoaNotaFiscalDTO
        {
            IdUsuario = grupo.Key,
            NomeCompleto = pessoa.NomeCompleto,
            Cpf = pessoa.Cpf,
            Dias = dias,
            TotalHoras = dias.Sum(d => d.HorasTrabalhadas ?? 0),
            TotalValor = dias.Sum(d => d.ValorDia)
        };
    }

    private static string DerivarSegmento(string? serie)
    {
        if (string.IsNullOrWhiteSpace(serie) || serie == "HIS")
            return NomeSegmentoSemSerie;
        if (serie.EndsWith("EF2", StringComparison.Ordinal))
            return "EF2";
        if (serie.EndsWith("EM", StringComparison.Ordinal))
            return "EM";
        return NomeSegmentoSemSerie;
    }
}
