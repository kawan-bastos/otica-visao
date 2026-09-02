using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OticaVisao.Web.Pages.Admin.Account;

[AllowAnonymous]
public sealed class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
    }
}
