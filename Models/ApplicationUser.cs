using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Flat.Models;

public class ApplicationUser : IdentityUser
{
    [Required, StringLength(80)]
    public string DisplayName { get; set; } = string.Empty;
}