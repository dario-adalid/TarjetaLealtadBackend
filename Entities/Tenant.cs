using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fidelidad.Api.Entities;

[Table("Tenants")]
public class Tenant
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(150)]
    public string BusinessName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(150)]
    public string OwnerEmail { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string? OwnerPhone { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Subdomain { get; set; } = string.Empty;
    
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    
    public int SubscriptionPlanId { get; set; }
    
    [ForeignKey("SubscriptionPlanId")]
    public SubscriptionPlan? SubscriptionPlan { get; set; }
    
    public int CurrentUsers { get; set; } = 0;
    
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "PendingPayment";
    
    public string? QrCodeUrl { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string TablePrefix { get; set; } = string.Empty;
    
    // Campos de Auditoría y Soft Delete
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public string UpdatedBy { get; set; } = "system";
    
    // Soft Delete: Si es NULL, el registro está activo. Si tiene fecha, fue "eliminado".
    public DateTime? DeletedAt { get; set; } 
}