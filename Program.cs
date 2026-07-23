using SistemaHEAVELYBackend.Data;
using SistemaHEAVELYBackend.Services;
using SistemaHEAVELYBackend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CloudinaryDotNet;
using SendGrid;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
var builder = WebApplication.CreateBuilder(args);

// 1. Banco de dados
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. JWT
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// 2.5. Cloudinary (upload de documentos)
var cloudinaryAccount = new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]
);
builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));

// 2.6. SendGrid (email de recuperação de senha)
builder.Services.AddSingleton<ISendGridClient>(
    new SendGridClient(builder.Configuration["SendGrid:ApiKey"]));

// 3. CORS — restrito às origens listadas em CORS_ORIGIN (fail-closed se não setada em produção)
var corsOrigins = (builder.Configuration["CORS_ORIGIN"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .ToList();

if (builder.Environment.IsDevelopment())
{
    corsOrigins.Add("http://localhost:5173");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirOrigensConfiguradas", policy =>
    {
        policy.WithOrigins(corsOrigins.ToArray())
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 4. Registra os Services (Limpo e sem repetições!)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventoService, EventoService>();
builder.Services.AddScoped<IAlocacaoService, AlocacaoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IPontoService, PontoService>();
builder.Services.AddScoped<IOcorrenciaService, OcorrenciaService>();
builder.Services.AddScoped<INotaFiscalService, NotaFiscalService>();

// 5. Configurações padrão da API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 6. Pipeline de execução
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PermitirOrigensConfiguradas");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();