namespace SistemaHEAVELYBackend.Services;

// ─── NEUTRALIZAÇÃO DO FUSO HORÁRIO DA RAILWAY ───────────────────
// Identifica o fuso correto do Brasil (compatível com Windows local e Linux da Railway)
// e converte um horário de Brasília (sem fuso) para UTC. Extraído de
// EventoService.ConverterHorarioBrasiliaParaUtc para ser reaproveitado também
// por NotaFiscalService — não duplicar esta lógica em nenhum outro lugar.
public static class FusoHorarioHelper
{
    public static DateTime BrasiliaParaUtc(DateTime horarioBrasilia)
    {
        var fusoBrasil = TimeZoneInfo.FindSystemTimeZoneById(
            Environment.OSVersion.Platform == PlatformID.Unix ? "America/Sao_Paulo" : "E. South America Standard Time");

        // Desvincula o horário vindo do formulário de qualquer fuso implícito
        var horarioSemFuso = DateTime.SpecifyKind(horarioBrasilia, DateTimeKind.Unspecified);

        // Converte para UTC considerando que a origem é o fuso de Brasília
        return TimeZoneInfo.ConvertTimeToUtc(horarioSemFuso, fusoBrasil);
    }
}
