using System.ComponentModel.DataAnnotations;

namespace AppSalesMetrics.Shared.Models;

public class ApplicationUserWithRolesDto : ApplicationUserDto
{
    public List<string>? Roles { get; set; }
}
