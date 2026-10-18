namespace Fidelidad.Api.Models;

public record RegisterCustomerRequest(string Name, string Email, string Phone, int TenantId);
public record CustomerResponse(int Id, string Name, string Identifier, decimal CurrentBalance);