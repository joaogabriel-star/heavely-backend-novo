namespace SistemaHEAVELYBackend.Services;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SistemaHEAVELYBackend.DTOs.Relatorios;

public class RelatorioService : IRelatorioService
{
    private readonly AppDbContext _context;

    public RelatorioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RelatorioEventoDTO> GerarDadosRelatorioAsync(
        int idEvento, ConfiguracaoRelatorioDTO config)
    {
        // Valida tipo de pagamento
        if (config.TipoPagamento != "PorHora" && config.TipoPagamento != "ValorFixo")
            throw new Exception("TipoPagamento deve ser 'PorHora' ou 'ValorFixo'.");

        // Busca o evento com todas as alocações e dados dos usuários
        var evento = await _context.EventosProvas
            .Include(e => e.Alocacos)
                .ThenInclude(a => a.IdUsuarioNavigation)
            .FirstOrDefaultAsync(e => e.IdEvento == idEvento);

        if (evento == null)
            throw new Exception("Evento não encontrado.");

        // Filtra só quem fez check-in (Presente)
        var presentes = evento.Alocacos
            .Where(a => a.StatusParticipacao == "Presente" ||
                       (a.CheckInTime.HasValue))
            .ToList();

        // Monta os itens do relatório
        var itens = presentes.Select(a =>
        {
            var horas = CalcularHoras(a.CheckInTime, a.CheckOutTime, evento.DataProva);
            var valor = CalcularValor(config, horas);

            return new RelatorioItemDTO
            {
                NomeCompleto = a.IdUsuarioNavigation.NomeCompleto,
                Cpf = a.IdUsuarioNavigation.Cpf,
                Email = a.IdUsuarioNavigation.Email,
                Celular = a.IdUsuarioNavigation.Celular,
                PapelEvento = a.PapelEvento,
                StatusParticipacao = a.StatusParticipacao,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                HorasTrabalhadas = horas,
                ValorAPagar = valor
            };
        })
        .OrderBy(i => i.PapelEvento)
        .ThenBy(i => i.NomeCompleto)
        .ToList();

        return new RelatorioEventoDTO
        {
            TituloProva = evento.TituloProva,
            LocalProva = evento.LocalProva ?? string.Empty,
            DataProva = evento.DataProva,
            HorarioFim = evento.HorarioFim,
            Participantes = itens,
            TotalPresentes = itens.Count,
            TotalHoras = itens.Sum(i => i.HorasTrabalhadas ?? 0),
            TotalValorGeral = itens.Sum(i => i.ValorAPagar)
        };
    }

    public async Task<byte[]> GerarPdfRelatorioAsync(
        int idEvento, ConfiguracaoRelatorioDTO config)
    {
        // Busca os dados primeiro
        var dados = await GerarDadosRelatorioAsync(idEvento, config);

        // Licença community do QuestPDF — obrigatório declarar
        QuestPDF.Settings.License = LicenseType.Community;

        // Monta o documento PDF
        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape()); // paisagem para caber mais colunas
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(9));

                // ── Cabeçalho ────────────────────────────────────────────────
                page.Header().Column(col =>
                {
                    col.Item().Text("RELATÓRIO DE PAGAMENTO")
                        .FontSize(16).Bold().AlignCenter();

                    col.Item().PaddingTop(4).Text(dados.TituloProva)
                        .FontSize(13).SemiBold().AlignCenter();

                    col.Item().PaddingTop(2).Row(row =>
                    {
                        row.RelativeItem().Text(
                            $"Local: {dados.LocalProva}").FontSize(9);

                        row.RelativeItem().Text(
                            $"Data: {dados.DataProva:dd/MM/yyyy HH:mm}" +
                            $"  até  {dados.HorarioFim:HH:mm}").FontSize(9).AlignRight();
                    });

                    col.Item().PaddingTop(4).LineHorizontal(1);
                });

                // ── Conteúdo ─────────────────────────────────────────────────
                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Agrupa por papel (Ledor / Fiscal)
                    var grupos = dados.Participantes
                        .GroupBy(p => p.PapelEvento);

                    foreach (var grupo in grupos)
                    {
                        // Título do grupo
                        col.Item().PaddingTop(10).Text(grupo.Key.ToUpper() + "ES")
                            .FontSize(11).Bold();

                        // Tabela
                        col.Item().PaddingTop(4).Table(tabela =>
                        {
                            // Define as colunas
                            tabela.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn(3); // Nome
                                cols.RelativeColumn(2); // CPF
                                cols.RelativeColumn(2); // Celular
                                cols.RelativeColumn(2); // Check-in
                                cols.RelativeColumn(2); // Check-out
                                cols.RelativeColumn(1); // Horas
                                cols.RelativeColumn(2); // Valor
                            });

                            // Cabeçalho da tabela
                            tabela.Header(header =>
                            {
                                var headerStyle = TextStyle.Default.FontSize(8).Bold();

                                header.Cell().Background("#2C3E50")
                                    .Padding(5).Text("Nome").Style(headerStyle)
                                    .FontColor(Colors.White);
                                header.Cell().Background("#2C3E50")
                                    .Padding(5).Text("CPF").Style(headerStyle)
                                    .FontColor(Colors.White);
                                header.Cell().Background("#2C3E50")
                                    .Padding(5).Text("Celular").Style(headerStyle)
                                    .FontColor(Colors.White);
                                header.Cell().Background("#2C3E50")
                                    .Padding(5).Text("Check-in").Style(headerStyle)
                                    .FontColor(Colors.White);
                                header.Cell().Background("#2C3E50")
                                    .Padding(5).Text("Check-out").Style(headerStyle)
                                    .FontColor(Colors.White);
                                header.Cell().Background("#2C3E50")
                                    .Padding(5).Text("Horas").Style(headerStyle)
                                    .FontColor(Colors.White);
                                header.Cell().Background("#2C3E50")
                                    .Padding(5).Text("Valor (R$)").Style(headerStyle)
                                    .FontColor(Colors.White);
                            });

                            // Linhas da tabela
                            var lista = grupo.ToList();
                            for (int i = 0; i < lista.Count; i++)
                            {
                                var item = lista[i];
                                // Alterna cor das linhas para facilitar leitura
                                var bg = i % 2 == 0 ? "#F8F9FA" : "#FFFFFF";

                                tabela.Cell().Background(bg).Padding(4)
                                    .Text(item.NomeCompleto);
                                tabela.Cell().Background(bg).Padding(4)
                                    .Text(FormatarCpf(item.Cpf));
                                tabela.Cell().Background(bg).Padding(4)
                                    .Text(item.Celular);
                                tabela.Cell().Background(bg).Padding(4)
                                    .Text(item.CheckInTime.HasValue
                                        ? item.CheckInTime.Value.ToString("HH:mm")
                                        : "-");
                                tabela.Cell().Background(bg).Padding(4)
                                    .Text(item.CheckOutTime.HasValue
                                        ? item.CheckOutTime.Value.ToString("HH:mm")
                                        : "-");
                                tabela.Cell().Background(bg).Padding(4)
                                    .Text(item.HorasTrabalhadas.HasValue
                                        ? $"{item.HorasTrabalhadas:F1}h"
                                        : "-");
                                tabela.Cell().Background(bg).Padding(4)
                                    .Text($"R$ {item.ValorAPagar:F2}").AlignRight();
                            }

                            // Linha de subtotal do grupo
                            var totalGrupo = grupo.Sum(p => p.ValorAPagar);
                            tabela.Cell().ColumnSpan(6).Background("#D5E8D4")
                                .Padding(4).Text($"Subtotal {grupo.Key}s").Bold();
                            tabela.Cell().Background("#D5E8D4")
                                .Padding(4).Text($"R$ {totalGrupo:F2}").Bold().AlignRight();
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
                            .Text("Total de Presentes").FontColor(Colors.White).Bold();
                        tabela.Cell().Background("#2C3E50").Padding(6)
                            .Text($"{dados.TotalPresentes}").FontColor(Colors.White)
                            .Bold().AlignRight();

                        tabela.Cell().Background("#34495E").Padding(6)
                            .Text("Total de Horas").FontColor(Colors.White).Bold();
                        tabela.Cell().Background("#34495E").Padding(6)
                            .Text($"{dados.TotalHoras:F1}h").FontColor(Colors.White)
                            .Bold().AlignRight();

                        tabela.Cell().Background("#27AE60").Padding(6)
                            .Text("TOTAL GERAL A PAGAR").FontColor(Colors.White).Bold()
                            .FontSize(11);
                        tabela.Cell().Background("#27AE60").Padding(6)
                            .Text($"R$ {dados.TotalValorGeral:F2}").FontColor(Colors.White)
                            .Bold().FontSize(11).AlignRight();
                    });
                });

                // ── Rodapé ───────────────────────────────────────────────────
                page.Footer().Row(row =>
                {
                    row.RelativeItem().Text(
                        $"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8);

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

        // Converte o documento para bytes e retorna
        return documento.GeneratePdf();
    }

    // ─── Métodos privados ─────────────────────────────────────────────────────

    private double? CalcularHoras(DateTime? checkIn, DateTime? checkOut, DateTime dataOficialProva)
    {
        return CalculoHorasHelper.Calcular(checkIn, checkOut, dataOficialProva);
    }

    private decimal CalcularValor(ConfiguracaoRelatorioDTO config, double? horas)
    {
        if (config.TipoPagamento == "ValorFixo")
            return config.ValorFixo;

        // PorHora — se não tem horas registradas, valor é zero
        if (!horas.HasValue) return 0;
        return Math.Round((decimal)horas.Value * config.ValorPorHora, 2);
    }

    private string FormatarCpf(string cpf)
    {
        // Transforma "12345678901" em "123.456.789-01"
        if (cpf.Length != 11) return cpf;
        return $"{cpf[..3]}.{cpf[3..6]}.{cpf[6..9]}-{cpf[9..]}";
    }
}