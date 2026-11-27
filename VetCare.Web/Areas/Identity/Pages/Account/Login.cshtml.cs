using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VetCare.DA;

namespace VetCare.Web.Areas.Identity.Pages.Account;

// Reemplaza la página de inicio de sesión que trae Identity por una en español
[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    public LoginModel(SignInManager<ApplicationUser> signInManager) => _signInManager = signInManager;

    [BindProperty]
    public EntradaLogin Input { get; set; } = new();

    public string? MensajeError { get; set; }

    public class EntradaLogin
    {
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    public void OnGet()
    {
    }

    // returnUrl llega cuando el usuario intentó abrir una página protegida (HU-03 CA-01)
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        if (!ModelState.IsValid) return Page();

        var resultado = await _signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

        if (resultado.Succeeded) return LocalRedirect(returnUrl);

        if (resultado.IsLockedOut)
        {
            MensajeError = "Su cuenta está desactivada. Contacte al administrador";
            return Page();
        }

        // HU-01 CA-02: mensaje genérico, sin decir qué dato falló
        MensajeError = "Correo o contraseña incorrectos";
        return Page();
    }
}