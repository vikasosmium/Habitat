using System.ComponentModel.DataAnnotations;

namespace Flat.Models;

public class RegisterFormModel
{
    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(40, MinimumLength = 3)]
    [RegularExpression("^[a-zA-Z0-9._-]+$", ErrorMessage = "Use letters, numbers, dots, dashes or underscores.")]
    public string LoginId { get; set; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password))]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class LoginFormModel
{
    [Required]
    public string LoginId { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}