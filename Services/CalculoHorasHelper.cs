namespace SistemaHEAVELYBackend.Services;

// Fórmula única de cálculo de horas trabalhadas, clampando o início ao horário
// oficial da prova (quem chega antes não ganha crédito pela espera; quem chega
// atrasado só é contado a partir da chegada real). Usada por PontoService,
// AlocacaoService e RelatorioService — não duplicar esta lógica em nenhum outro lugar.
public static class CalculoHorasHelper
{
    public static double? Calcular(DateTime? checkIn, DateTime? checkOut, DateTime dataOficialProva)
    {
        if (!checkIn.HasValue || !checkOut.HasValue)
            return null;

        var inicioRealCalculo = checkIn.Value < dataOficialProva ? dataOficialProva : checkIn.Value;

        if (checkOut.Value < inicioRealCalculo)
            return 0;

        return Math.Round((checkOut.Value - inicioRealCalculo).TotalHours, 2);
    }
}
