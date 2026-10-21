using System.ComponentModel.DataAnnotations;

namespace ProxyCraftPlus.Api.Contracts;

public class CredentialsRequest
{
    [Required, StringLength(254, MinimumLength = 3), RegularExpression(@"^\S+$", ErrorMessage = "Use a username or an email address without spaces.")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 12)]
    public string Password { get; set; } = string.Empty;
}
