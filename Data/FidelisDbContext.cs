using Fidelidad.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fidelidad.Api.Data;

public class FidelisDbContext : DbContext
{
    public FidelisDbContext(DbContextOptions<FidelisDbContext> options) : base(options)
    {
    }

    // Aquí declaras las tablas globales
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<Tenant> Tenants { get; set; }

    // NOTA: Las tablas con prefijo (tenant_1_...) las manejaremos con 
    // consultas SQL directas o Dapper más adelante para mantener la simplicidad, 
    // ya que EF Core no maneja bien los nombres de tablas dinámicos por defecto.
}