using Fidelidad.Api.Data;
using Fidelidad.Api.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Fidelidad.Api.Services;

public interface ILoyaltyService
{
    Task<CustomerResponse?> RegisterCustomerAsync(int tenantId, RegisterCustomerRequest request);
    Task<CustomerResponse?> GetCustomerAsync(int tenantId, string identifier);
    Task<ScanResponse> ProcessScanAsync(int tenantId, string identifier);
    Task<List<CustomerResponse>> GetAllCustomersAsync(int tenantId);
}

public class LoyaltyService : ILoyaltyService
{
    private readonly FidelisDbContext _db;
    private readonly ILogger<LoyaltyService> _logger;

    public LoyaltyService(FidelisDbContext db, ILogger<LoyaltyService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<CustomerResponse?> RegisterCustomerAsync(int tenantId, RegisterCustomerRequest request)
    {
        var prefix = await GetTenantPrefixAsync(tenantId);
        if (prefix == null) return null;

        var identifier = Guid.NewGuid().ToString("N");
        var tableName = $"{prefix}Customers";

        // INSERT con MySqlParameter explícitos
        var insertSql = $@"
            INSERT INTO {tableName} (TenantId, Name, Email, Phone, Identifier, CurrentBalance, TotalVisits, JoinedAt, CreatedAt, CreatedBy)
            VALUES (@TenantId, @Name, @Email, @Phone, @Identifier, 0, 0, NOW(), NOW(), 'system')";

        await _db.Database.ExecuteSqlRawAsync(insertSql, new MySqlParameter[]
        {
            new("@TenantId", tenantId),
            new("@Name", request.Name),
            new("@Email", (object?)request.Email ?? DBNull.Value),
            new("@Phone", (object?)request.Phone ?? DBNull.Value),
            new("@Identifier", identifier)
        });

        // SELECT por separado
        var selectSql = $@"
            SELECT Id, Name, Identifier, CurrentBalance, TotalVisits 
            FROM {tableName} 
            WHERE Identifier = @Identifier";

        var customer = await _db.Database
            .SqlQueryRaw<CustomerDbRecord>(selectSql, new MySqlParameter("@Identifier", identifier))
            .FirstOrDefaultAsync();

        return customer != null ? MapToResponse(customer) : null;
    }

    public async Task<CustomerResponse?> GetCustomerAsync(int tenantId, string identifier)
    {
        var prefix = await GetTenantPrefixAsync(tenantId);
        if (prefix == null) return null;

        var tableName = $"{prefix}Customers";
        var sql = $@"
            SELECT Id, Name, Identifier, CurrentBalance, TotalVisits 
            FROM {tableName} 
            WHERE Identifier = @Identifier AND DeletedAt IS NULL";

        var customer = await _db.Database
            .SqlQueryRaw<CustomerDbRecord>(sql, new MySqlParameter("@Identifier", identifier))
            .FirstOrDefaultAsync();

        return customer != null ? MapToResponse(customer) : null;
    }

    public async Task<List<CustomerResponse>> GetAllCustomersAsync(int tenantId)
    {
        var prefix = await GetTenantPrefixAsync(tenantId);
        if (prefix == null) return new List<CustomerResponse>();

        var tableName = $"{prefix}Customers";
        var sql = $@"
            SELECT Id, Name, Identifier, CurrentBalance, TotalVisits 
            FROM {tableName} 
            WHERE DeletedAt IS NULL
            ORDER BY JoinedAt DESC";

        var customers = await _db.Database
            .SqlQueryRaw<CustomerDbRecord>(sql)
            .ToListAsync();

        return customers.Select(MapToResponse).ToList();
    }

    public async Task<ScanResponse> ProcessScanAsync(int tenantId, string identifier)
    {
        var prefix = await GetTenantPrefixAsync(tenantId);
        if (prefix == null) return new ScanResponse(false, 0, false, "Negocio no encontrado");

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var tableNameCustomers = $"{prefix}Customers";
            var tableNameTransactions = $"{prefix}Transactions";

            var customer = await _db.Database
                .SqlQueryRaw<CustomerDbRecord>(
                    $"SELECT Id, Name, Identifier, CurrentBalance, TotalVisits FROM {tableNameCustomers} WHERE Identifier = @Identifier AND DeletedAt IS NULL",
                    new MySqlParameter("@Identifier", identifier)
                ).FirstOrDefaultAsync();

            if (customer == null)
                return new ScanResponse(false, 0, false, "Cliente no registrado en este negocio");

            var oldBalance = customer.CurrentBalance;
            var newBalance = oldBalance + 1;
            var newVisits = customer.TotalVisits + 1;
            var rewardUnlocked = newBalance >= 10;

            var sqlTrans = $@"
                INSERT INTO {tableNameTransactions} (TenantId, CustomerId, Type, Amount, BalanceBefore, BalanceAfter, Reference, Description, CreatedAt, CreatedBy)
                VALUES (@TenantId, @CustomerId, 'Earn', 1, @BalanceBefore, @BalanceAfter, 'SCAN', 'Visita registrada', NOW(), 'system')";

            await _db.Database.ExecuteSqlRawAsync(sqlTrans, new MySqlParameter[]
            {
                new("@TenantId", tenantId),
                new("@CustomerId", customer.Id),
                new("@BalanceBefore", oldBalance),
                new("@BalanceAfter", newBalance)
            });

            var sqlUpdate = $@"
                UPDATE {tableNameCustomers} 
                SET CurrentBalance = @NewBalance, TotalVisits = @NewVisits, LastVisit = NOW(), UpdatedAt = NOW()
                WHERE Id = @CustomerId";

            await _db.Database.ExecuteSqlRawAsync(sqlUpdate, new MySqlParameter[]
            {
                new("@NewBalance", newBalance),
                new("@NewVisits", newVisits),
                new("@CustomerId", customer.Id)
            });

            await transaction.CommitAsync();

            return new ScanResponse(
                Success: true,
                NewBalance: newBalance,
                RewardUnlocked: rewardUnlocked,
                Message: rewardUnlocked ? "¡Felicidades! Has alcanzado tu recompensa." : "Visita registrada correctamente."
            );
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al procesar escaneo para el cliente {Identifier}", identifier);
            return new ScanResponse(false, 0, false, "Error interno al procesar la visita");
        }
    }

    private async Task<string?> GetTenantPrefixAsync(int tenantId)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId);
        return tenant?.TablePrefix;
    }

    private CustomerResponse MapToResponse(CustomerDbRecord record) => new(
        Id: record.Id,
        Name: record.Name,
        Identifier: record.Identifier,
        CurrentBalance: record.CurrentBalance
    );


}