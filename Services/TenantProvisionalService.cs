using Fidelidad.Api.Data;
using Fidelidad.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Fidelidad.Api.Services;

public interface ITenantProvisioningService
{
    /// <summary>
    /// Registra un nuevo tenant y crea sus tablas en una transacción atómica.
    /// </summary>
    Task<Tenant> ProvisionTenantAsync(Tenant tenant);
    
    /// <summary>
    /// Verifica si las tablas de un tenant ya existen (útil para evitar duplicados).
    /// </summary>
    Task<bool> TenantTablesExistAsync(int tenantId);
}

public class TenantProvisioningService : ITenantProvisioningService
{
    private readonly FidelisDbContext _db;
    private readonly ILogger<TenantProvisioningService> _logger;

    public TenantProvisioningService(FidelisDbContext db, ILogger<TenantProvisioningService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Tenant> ProvisionTenantAsync(Tenant tenant)
    {
        // Usamos una transacción para garantizar que todo se haga o nada se haga
        using var transaction = await _db.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Insertar el tenant en la tabla global
            _db.Tenants.Add(tenant);
            await _db.SaveChangesAsync();

            // 2. Definir el prefijo con el ID real
            var prefix = $"tenant_{tenant.Id}_";
            tenant.TablePrefix = prefix;
            await _db.SaveChangesAsync();

            // 3. Crear las 6 tablas del tenant
            await CreateTenantTablesAsync(prefix);

            // 4. Confirmar la transacción
            await transaction.CommitAsync();

            _logger.LogInformation("Tenant {TenantId} ({BusinessName}) provisionado exitosamente.", 
                tenant.Id, tenant.BusinessName);

            return tenant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al provisionar tenant {TenantId}. Haciendo rollback.", tenant.Id);
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> TenantTablesExistAsync(int tenantId)
    {
        var prefix = $"tenant_{tenantId}_";
        var tableName = $"{prefix}Customers"; // Usamos Customers como referencia
        
        // Verificar si la tabla existe en INFORMATION_SCHEMA
        var exists = await _db.Database
            .SqlQueryRaw<int>(
                @"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                  WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = {0}", 
                tableName)
            .FirstOrDefaultAsync();
        
        return exists > 0;
    }

    private async Task CreateTenantTablesAsync(string prefix)
    {
        foreach (var template in TenantTableTemplates.AllTables)
        {
            // Reemplazar el marcador {PREFIX} por el prefijo real
            var sql = template.Replace("{PREFIX}", prefix);
            
            // Ejecutar el CREATE TABLE
            await _db.Database.ExecuteSqlRawAsync(sql);
            
            _logger.LogDebug("Tabla {Prefix} creada exitosamente.", prefix);
        }
    }
}