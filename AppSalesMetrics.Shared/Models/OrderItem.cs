using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AppSalesMetrics.Shared.Models;

[DataContract]
public class OrderItem
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    public long? ProductId { get; set; }

    [DataMember]
    public long? Quantity { get; set; }

    [DataMember]
    public decimal? UnitPrice { get; set; }

    [DataMember]
    public DateTime? CreatedDate { get; set; }

    [DataMember]
    public DateTime? ModifiedDate { get; set; }

    [DataMember]
    public Product? Product { get; set; }
}
