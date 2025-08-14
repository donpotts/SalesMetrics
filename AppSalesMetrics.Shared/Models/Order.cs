using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AppSalesMetrics.Shared.Models;

[DataContract]
public class Order
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    public long? CustomerId { get; set; }

    [DataMember]
    public DateTime? OrderDate { get; set; }

    [DataMember]
    public decimal? TotalAmount { get; set; }

    [DataMember]
    public string? Status { get; set; }

    [DataMember]
    public string? Notes { get; set; }

    [DataMember]
    public DateTime? CreatedDate { get; set; }

    [DataMember]
    public DateTime? ModifiedDate { get; set; }

    [DataMember]
    public Customer? Customer { get; set; }

    [DataMember]
    public List<OrderItem>? OrderItem { get; set; }
}
