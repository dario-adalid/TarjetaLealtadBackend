namespace Fidelidad.Api.Models;

public record ScanRequest(string CustomerIdentifier, int TenantId);
public record ScanResponse(bool Success, decimal NewBalance, bool RewardUnlocked, string Message);
public record CreatePromotionRequest(int TenantId, string Name, string Description, string ConfigJson, int MinVisitsRequired);