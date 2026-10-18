namespace Fidelidad.Api.Models;

public record RegisterBusinessRequest(string BusinessName, string OwnerEmail, string Subdomain, int PlanId);
public record BusinessResponse(int Id, string BusinessName, string Status, string QrCodeUrl);