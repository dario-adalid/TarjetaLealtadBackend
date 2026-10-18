using Fidelidad.Api.Data;
using Fidelidad.Api.Endpoints;
using Fidelidad.Api.Services;
using Microsoft.EntityFrameworkCore; 

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE SERVICIOS (Swashbuckle)
// ==========================================
builder.Services.AddDbContext<FidelisDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(10, 6, 0)) // Ajusta a tu versión de MariaDB (ej: 10.6, 10.11)
    ));

builder.Services.AddScoped<ITenantProvisioningService, TenantProvisioningService>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>(); 

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); // Esto genera el JSON automáticamente

var app = builder.Build();

// ==========================================
// 2. CONFIGURACIÓN DEL PIPELINE
// ==========================================
// Quitamos el 'if' de Development temporalmente para asegurar que funcione en tu local
app.UseSwagger(); // Habilita el middleware que sirve el JSON
app.UseSwaggerUI(c =>
{
    // ESTA es la ruta correcta que genera AddSwaggerGen()
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fidelidad API v1");
    c.RoutePrefix = "swagger"; // La URL será http://localhost:puerto/swagger
});

app.UseHttpsRedirection();

// ==========================================
// 3. MAPEO DE ENDPOINTS
// ==========================================
app.MapBusinessEndpoints();
app.MapCustomerEndpoints();
app.MapLoyaltyEndpoints();
app.MapDashboardEndpoints();

app.Run();