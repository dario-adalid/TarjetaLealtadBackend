using Fidelidad.Api.Models;
using Fidelidad.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fidelidad.Api.Endpoints;

public static class LoyaltyEndpoints
{
    public static void MapLoyaltyEndpoints(this WebApplication app)
    {
        app.MapPost("/api/loyalty/scan", async (
            [FromBody] ScanRequest request, 
            ILoyaltyService loyaltyService) =>
        {
            var result = await loyaltyService.ProcessScanAsync(request.TenantId, request.CustomerIdentifier);
            
            if (!result.Success)
                return Results.BadRequest(result);

            return Results.Ok(result);
        })
        .WithName("ScanLoyalty")
        .WithTags("Lealtad");
    }
}