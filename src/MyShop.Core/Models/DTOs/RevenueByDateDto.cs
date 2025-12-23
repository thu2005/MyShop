using System.Text.Json.Serialization;

namespace MyShop.Core.Models.DTOs;

/// <summary>
/// Revenue by date DTO for chart data
/// </summary>
public class RevenueByDateDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }

    [JsonPropertyName("orders")]
    public int Orders { get; set; }

    /// <summary>
    /// Helper property to parse date string to DateTime for UI binding
    /// </summary>
    public DateTime ParsedDate => DateTime.Parse(Date);
}
