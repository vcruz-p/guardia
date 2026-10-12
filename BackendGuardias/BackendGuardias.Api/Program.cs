using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using BackendGuardias.Api.Data;
using BackendGuardias.Api.Middleware;
using BackendGuardias.Api.Repositories.GruposGuardia;
using BackendGuardias.Api.Services.GruposGuardia;
using BackendGuardias.Api.Repositories.PersonasUnidadOrganizativa;
using BackendGuardias.Api.Services.PersonasUnidadOrganizativa;
using BackendGuardias.Api.Repositories.EstadosParte;
using BackendGuardias.Api.Services.EstadosParte;
using BackendGuardias.Api.Repositories.ResponsablesAprobacionGuardia;
using BackendGuardias.Api.Services.ResponsablesAprobacionGuardia;
using BackendGuardias.Api.Repositories.PlanGuardia;
using BackendGuardias.Api.Services.PlanGuardia;


Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5206);
});


// ============================================================
// CONEXIÓN A BASE DE DATOS
// ============================================================

// La cadena puede configurarse de cualquiera de estas formas:
// 1. appsettings.json / appsettings.Development.json:
//    ConnectionStrings:GuardiasConnection
// 2. Variable de entorno estándar:
//    ConnectionStrings__GuardiasConnection
// 3. Variable de entorno alternativa en el archivo .env:
//    GUARDIAS_CONNECTION_STRING
var connectionString =
    builder.Configuration.GetConnectionString("GuardiasConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString =
        Environment.GetEnvironmentVariable("GUARDIAS_CONNECTION_STRING");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Falta configurar la conexión MySQL 'GuardiasConnection'. " +
        "Cree BackendGuardias.Api/.env a partir de .env.example y configure " +
        "GUARDIAS_CONNECTION_STRING, o defina la variable de entorno " +
        "ConnectionStrings__GuardiasConnection. No incluya credenciales reales en Git."
    );
}

builder.Services.AddDbContext<GuardiasDbContext>(options =>
{
    options.UseMySQL(connectionString);
});


// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirTodos", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


// ============================================================
// SERVICIOS
// ============================================================

builder.Services.AddScoped<
    IGrupoGuardiaService,
    GrupoGuardiaService
>();
builder.Services.AddScoped<
    IGrupoGuardiaRepository,
    GrupoGuardiaRepository
>();


builder.Services.AddScoped<
    IPersonasUnidadOrganizativaRepository,
    PersonasUnidadOrganizativaRepository
>();

builder.Services.AddScoped<
    IPersonasUnidadOrganizativaService,
    PersonasUnidadOrganizativaService
>();

builder.Services.AddScoped<
    IEstadoParteRepository,
    EstadoParteRepository>();

builder.Services.AddScoped<
    IEstadoParteService,
    EstadoParteService>();

    builder.Services.AddScoped<
    IResponsableAprobacionGuardiaRepository,
    ResponsableAprobacionGuardiaRepository>();

builder.Services.AddScoped<
    IResponsableAprobacionGuardiaService,
    ResponsableAprobacionGuardiaService>();

    builder.Services.AddScoped<
    IPlanGuardiaRepository,
    PlanGuardiaRepository>();

builder.Services.AddScoped<
    IPlanGuardiaService,
    PlanGuardiaService>();

builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();


// ============================================================
// CONTROLLERS
// ============================================================

builder.Services.AddControllers();
builder.Services.AddAuthorization();


// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();


// ============================================================
// BUILD
// ============================================================

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();


// ============================================================
// SWAGGER
// ============================================================

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "Backend Guardias API v1"
    );

    options.RoutePrefix = "swagger";
});

app.MapGet("/", () => Results.Redirect("/swagger"));


// ============================================================
// CORS
// ============================================================

app.UseCors("PermitirTodos");


// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();


// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();


// ============================================================
// RUN
// ============================================================

app.Run();