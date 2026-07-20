using Microsoft.AspNetCore.Http;

namespace SistemaHEAVELYBackend.DTOs.Auth;

public class EnviarDocumentosDTO
{
    public IFormFile? Diploma { get; set; }
    public IFormFile? NadaConsta { get; set; }
}
