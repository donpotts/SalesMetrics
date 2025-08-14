using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace AppSalesMetrics.Shared.Models;

[DataContract]
public class Customer
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    public string? Name { get; set; }

    [DataMember]
    public string? Email { get; set; }

    [DataMember]
    public string? Phone { get; set; }

    [DataMember]
    public string? Address { get; set; }

    [DataMember]
    public string? Notes { get; set; }

    [DataMember]
    public DateTime? CreatedDate { get; set; }

    [DataMember]
    public DateTime? ModifiedDate { get; set; }
}
