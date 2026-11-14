using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VetCare.Web.Data;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(100)]
    public string NombreCompleto { get; set; } = string.Empty;
}