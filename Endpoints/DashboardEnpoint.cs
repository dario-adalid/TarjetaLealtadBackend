namespace Fidelidad.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this WebApplication app)
    {
        app.MapGet("/api/dashboard/stats", (int tenantId) =>
        {
            // TODO: Consultar tabla tenant_{id}_DailyStats
            var stats = new
            {
                TenantId = tenantId,
                TotalCustomers = 150,
                ActiveCustomersLast30Days = 85,
                TotalVisitsToday = 12,
                TotalRewardsRedeemed = 5
            };
            return Results.Ok(stats);
        })
        .WithName("GetDashboardStats")
        .WithTags("Dashboard");
    }
}