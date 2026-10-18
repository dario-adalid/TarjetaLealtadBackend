using Fidelidad.Api.Models;
using Fidelidad.Api.Entities;
using Fidelidad.Api.Services;
using Fidelidad.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fidelidad.Api.Endpoints;

public static class BusinessEndpoints
{
    public static void MapBusinessEndpoints(this WebApplication app)
    {
        // ==========================================
        // 1. REGISTRAR NEGOCIO (POST)
        // ==========================================
        app.MapPost("/api/businesses/register", async (
            [FromBody] RegisterBusinessRequest request, 
            ITenantProvisioningService provisioningService) =>
        {
            try
            {
                var newTenant = new Tenant
                {
                    BusinessName = request.BusinessName,
                    OwnerEmail = request.OwnerEmail,
                    Subdomain = request.Subdomain,
                    SubscriptionPlanId = request.PlanId,
                    Status = "PendingPayment",
                    CurrentUsers = 0
                };

                var provisionedTenant = await provisioningService.ProvisionTenantAsync(newTenant);

                var response = new BusinessResponse(
                    Id: provisionedTenant.Id,
                    BusinessName: provisionedTenant.BusinessName,
                    Status: provisionedTenant.Status,
                    QrCodeUrl: $"https://api.fidelis.app/qr/{provisionedTenant.Id}"
                );

                return Results.Created($"/api/businesses/{provisionedTenant.Id}", response);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Error al registrar el negocio: {ex.Message}",
                    statusCode: 500);
            }
        })
        .WithName("RegisterBusiness")
        .WithTags("Negocios");

        // ==========================================
        // 2. OBTENER NEGOCIO POR ID (GET)
        // ==========================================
        app.MapGet("/api/businesses/{id}", async (int id, FidelisDbContext db) =>
        {
            var tenant = await db.Tenants.FindAsync(id);
            
            // Verificamos que exista y que no esté eliminado (Soft Delete)
            if (tenant is null || tenant.DeletedAt != null)
                return Results.NotFound();

            var response = new BusinessResponse(
                Id: tenant.Id,
                BusinessName: tenant.BusinessName,
                Status: tenant.Status,
                QrCodeUrl: tenant.QrCodeUrl ?? $"https://api.fidelis.app/qr/{tenant.Id}"
            );

            return Results.Ok(response);
        })
        .WithName("GetBusiness")
        .WithTags("Negocios");

        // ==========================================
        // 3. OBTENER TODOS LOS NEGOCIOS (GET ALL)
        // ==========================================
        app.MapGet("/api/businesses", async (FidelisDbContext db) =>
        {
            var tenants = await db.Tenants
                .Where(t => t.DeletedAt == null) // Solo los activos
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new BusinessResponse(
                    t.Id,
                    t.BusinessName,
                    t.Status,
                    t.QrCodeUrl ?? $"https://api.fidelis.app/qr/{t.Id}"
                ))
                .ToListAsync();

            return Results.Ok(tenants);
        })
        .WithName("GetAllBusinesses")
        .WithTags("Negocios");
        
    } 
} 