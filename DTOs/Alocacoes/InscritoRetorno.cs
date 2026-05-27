namespace SistemaHEAVELYBackend.DTOs.Alocacoes
{
    public class InscritoRetornoDTO
    {
        public int IdAlocacao { get; set; }
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 
        public string PapelEvento { get; set; } = string.Empty;
        public string StatusParticipacao { get; set; } = string.Empty;
        public string SalaDesignada { get; set; } = string.Empty;
    }
}