namespace Fidelidad.Api.Data;

/// <summary>
/// Plantillas SQL para crear las tablas de cada tenant.
/// Usa {PREFIX} como marcador de posición que será reemplazado por "tenant_{id}_".
/// </summary>
public static class TenantTableTemplates
{
    public const string BusinessSettings = @"
        CREATE TABLE {PREFIX}BusinessSettings (
            Id INT PRIMARY KEY AUTO_INCREMENT,
            TenantId INT NOT NULL,
            BusinessName VARCHAR(150) NOT NULL,
            LoyaltyType ENUM('STAMPS', 'POINTS', 'CASHBACK', 'FREE_PRODUCT', 'DISCOUNT') NOT NULL,
            EarnRule JSON NOT NULL,
            RewardThreshold DECIMAL(10,2) NOT NULL,
            RewardDescription VARCHAR(255) NOT NULL,
            Currency VARCHAR(3) DEFAULT 'USD',
            IsActive BOOLEAN DEFAULT TRUE,
            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            CreatedBy VARCHAR(100) DEFAULT 'system',
            UpdatedBy VARCHAR(100) DEFAULT 'system',
            INDEX idx_tenant (TenantId),
            INDEX idx_active (IsActive)
        )";

    public const string Customers = @"
        CREATE TABLE {PREFIX}Customers (
            Id INT PRIMARY KEY AUTO_INCREMENT,
            TenantId INT NOT NULL,
            Name VARCHAR(150) NOT NULL,
            Email VARCHAR(150),
            Phone VARCHAR(20),
            Identifier VARCHAR(100) NOT NULL,
            WalletPassId VARCHAR(200),
            CurrentBalance DECIMAL(10,2) DEFAULT 0,
            TotalVisits INT DEFAULT 0,
            LastVisit TIMESTAMP NULL,
            JoinedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            CreatedBy VARCHAR(100) DEFAULT 'system',
            UpdatedBy VARCHAR(100) DEFAULT 'system',
            DeletedAt TIMESTAMP NULL,
            UNIQUE KEY uk_identifier_tenant (Identifier, TenantId),
            INDEX idx_tenant (TenantId),
            INDEX idx_identifier (Identifier),
            INDEX idx_email (Email),
            INDEX idx_phone (Phone),
            INDEX idx_last_visit (LastVisit),
            INDEX idx_balance (CurrentBalance)
        )";

    public const string Transactions = @"
        CREATE TABLE {PREFIX}Transactions (
            Id INT PRIMARY KEY AUTO_INCREMENT,
            TenantId INT NOT NULL,
            CustomerId INT NOT NULL,
            Type ENUM('Earn', 'Redeem', 'ManualAdjust', 'Expire') NOT NULL,
            Amount DECIMAL(10,2) NOT NULL,
            BalanceBefore DECIMAL(10,2) NOT NULL,
            BalanceAfter DECIMAL(10,2) NOT NULL,
            Reference VARCHAR(100),
            Description VARCHAR(255),
            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            CreatedBy VARCHAR(100) DEFAULT 'system',
            INDEX idx_tenant (TenantId),
            INDEX idx_customer (CustomerId),
            INDEX idx_type (Type),
            INDEX idx_created_at (CreatedAt),
            INDEX idx_reference (Reference)
        )";

    public const string Promotions = @"
        CREATE TABLE {PREFIX}Promotions (
            Id INT PRIMARY KEY AUTO_INCREMENT,
            TenantId INT NOT NULL,
            Name VARCHAR(150) NOT NULL,
            Description TEXT,
            Config JSON NOT NULL,
            MinVisitsRequired INT DEFAULT 0,
            MaxRedemptions INT NULL,
            CurrentRedemptions INT DEFAULT 0,
            IsActive BOOLEAN DEFAULT TRUE,
            StartDate DATE NOT NULL,
            EndDate DATE NOT NULL,
            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            CreatedBy VARCHAR(100) DEFAULT 'system',
            UpdatedBy VARCHAR(100) DEFAULT 'system',
            DeletedAt TIMESTAMP NULL,
            INDEX idx_tenant (TenantId),
            INDEX idx_active (IsActive),
            INDEX idx_dates (StartDate, EndDate),
            INDEX idx_visits_required (MinVisitsRequired)
        )";

    public const string PromotionRedemptions = @"
        CREATE TABLE {PREFIX}PromotionRedemptions (
            Id INT PRIMARY KEY AUTO_INCREMENT,
            TenantId INT NOT NULL,
            PromotionId INT NOT NULL,
            CustomerId INT NOT NULL,
            RedeemedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            Reference VARCHAR(100),
            Notes TEXT,
            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            CreatedBy VARCHAR(100) DEFAULT 'system',
            INDEX idx_tenant (TenantId),
            INDEX idx_promotion (PromotionId),
            INDEX idx_customer (CustomerId),
            INDEX idx_redeemed_at (RedeemedAt)
        )";

    public const string DailyStats = @"
        CREATE TABLE {PREFIX}DailyStats (
            Id INT PRIMARY KEY AUTO_INCREMENT,
            TenantId INT NOT NULL,
            StatDate DATE NOT NULL,
            TotalCustomers INT DEFAULT 0,
            ActiveCustomers INT DEFAULT 0,
            NewCustomers INT DEFAULT 0,
            TotalVisits INT DEFAULT 0,
            TotalTransactions INT DEFAULT 0,
            TotalEarned DECIMAL(10,2) DEFAULT 0,
            TotalRedeemed DECIMAL(10,2) DEFAULT 0,
            TotalPromotionsRedeemed INT DEFAULT 0,
            CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
            UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
            UNIQUE KEY uk_tenant_date (TenantId, StatDate),
            INDEX idx_tenant (TenantId),
            INDEX idx_date (StatDate)
        )";

    /// <summary>
    /// Lista de todas las tablas que se deben crear para cada tenant.
    /// </summary>
    public static readonly string[] AllTables = new[]
    {
        BusinessSettings,
        Customers,
        Transactions,
        Promotions,
        PromotionRedemptions,
        DailyStats
    };
}