// DTOs/Ponto/RegistrarPontoDTO.cs
namespace SistemaHEAVELYBackend.DTOs.Ponto
{
    public class RegistrarPontoDTO
    {
        public int    IdEvento     { get; set; }
        public string Tipo         { get; set; } = string.Empty; // "entrada" | "saida"
        public string? TokenQRCode { get; set; } // obrigatório só na saída
    }

    public class PontoRespostaDTO
    {
        public bool    Sucesso          { get; set; }
        public string  Mensagem         { get; set; } = string.Empty;
        public string? HoraRegistrada   { get; set; }
        public string? HoraInicioCalculo{ get; set; } // hora real que o cálculo começa
        public double? HorasTrabalhadas { get; set; } // só na saída
    }

    public class GerarQRCodeDTO
    {
        public int IdEvento { get; set; }
    }

    public class QRCodeRespostaDTO
    {
        public string Token     { get; set; } = string.Empty;
        public string QRCodeUrl { get; set; } = string.Empty; // base64 da imagem
        public string ExpiraEm  { get; set; } = string.Empty;
    }
}