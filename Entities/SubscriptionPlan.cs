using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fidelidad.Api.Entities;

[Table("SubscriptionPlans")]
public class SubscriptionPlan
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public int MaxUsers { get; set; }
    public decimal MonthlyPrice { get; set; }
    public string? Features { get; set; } // JSON como string
    public bool IsActive { get; set; } = true;
}