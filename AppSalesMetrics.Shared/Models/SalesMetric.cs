using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AppSalesMetrics.Shared.Models;

[DataContract]
public class SalesMetric
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    public DateTime? MetricDate { get; set; }

    [DataMember]
    public decimal? TotalSales { get; set; }

    [DataMember]
    public long? TotalOrders { get; set; }

    [DataMember]
    public long? TotalCustomers { get; set; }

    [DataMember]
    public long? TopProductId { get; set; }

    [DataMember]
    public DateTime? CreatedDate { get; set; }

    [DataMember]
    public DateTime? ModifiedDate { get; set; }

    [DataMember]
    public string? IsLocked { get; set; }
}
