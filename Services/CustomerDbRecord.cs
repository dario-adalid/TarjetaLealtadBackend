namespace Fidelidad.Api.Services;

// Clase interna para mapear el resultado del SQL crudo
public class CustomerDbRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public int TotalVisits { get; set; }
}