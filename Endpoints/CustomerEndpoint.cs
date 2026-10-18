using Fidelidad.Api.Models;
using Fidelidad.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fidelidad.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this WebApplication app)
    {
        app.MapPost("/api/customers/register", async (
            [FromBody] RegisterCustomerRequest request, 
            ILoyaltyService loyaltyService) =>
        {
            var customer = await loyaltyService.RegisterCustomerAsync(request.TenantId, request);
            
            if (customer == null)
                return Results.NotFound("El negocio especificado no existe.");

            return Results.Created($"/api/customers/{customer.Identifier}", customer);
        })
        .WithName("RegisterCustomer")
        .WithTags("Clientes");

        app.MapGet("/api/customers", async (
            [FromQuery] int tenantId, 
            ILoyaltyService loyaltyService) =>
        {
            var customers = await loyaltyService.GetAllCustomersAsync(tenantId);
            return Results.Ok(customers);
        })
        .WithName("GetAllCustomers")
        .WithTags("Clientes");

        app.MapGet("/api/customers/{identifier}", async (
            string identifier, 
            [FromQuery] int tenantId, 
            ILoyaltyService loyaltyService) =>
        {
            var customer = await loyaltyService.GetCustomerAsync(tenantId, identifier);
            
            if (customer == null)
                return Results.NotFound("Cliente no encontrado.");

            return Results.Ok(customer);
        })
        .WithName("GetCustomer")
        .WithTags("Clientes");
    }
}