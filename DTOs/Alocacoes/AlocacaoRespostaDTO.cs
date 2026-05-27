namespace SistemaHEAVELYBackend.DTOs.Alocacoes
{
    public class AlocacaoRespostaDTO
    {
        public int IdAlocacao { get; set; }
        public int IdEvento { get; set; }
        public string TituloProva { get; set; } = string.Empty;
        public string NomeUsuario { get; set; } = string.Empty;
        public string PapelEvento { get; set; } = string.Empty;
        public string StatusParticipacao { get; set; } = string.Empty;
        public int? PosicaoReserva { get; set; }
        public DateTime DataInscricao { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public double? HorasTrabalhadas { get; set; } 
        
        // 🚀 As colunas novas que fizemos para a sala do candidato!
        public string? SalaDesignada { get; set; }
        public string? Observacoes { get; set; }
    }
}