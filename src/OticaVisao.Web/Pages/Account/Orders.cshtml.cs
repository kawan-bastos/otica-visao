using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OticaVisao.Domain.LaboratoryOrders;
using OticaVisao.Domain.Sales;
using OticaVisao.Infrastructure.Authentication;
using OticaVisao.Infrastructure.Persistence;

namespace OticaVisao.Web.Pages.Account;
[Authorize]
public sealed class OrdersModel(ApplicationDbContext context, UserManager<ApplicationUser> users) : PageModel
{
    public IReadOnlyList<CustomerSale> Sales { get; private set; } = [];
    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (User.IsInRole(AdminAuthorization.Role)) return RedirectToPage(AdminAuthorization.GetPanelLandingPage(User));
        var userId = Guid.Parse(users.GetUserId(User) ?? throw new InvalidOperationException("Usuário não encontrado."));
        var customerId = await context.Customers.Where(x => x.AccountUserId == userId).Select(x => x.Id).SingleOrDefaultAsync(ct);
        if (customerId == Guid.Empty) return Page();
        var sales = await context.Sales.AsNoTracking().Include(x => x.Items).Where(x => x.CustomerId == customerId && x.Status != SaleStatus.Cancelled).OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
        var saleIds = sales.Select(x => x.Id).ToArray();
        var orders = await context.LaboratoryOrders.AsNoTracking().Where(x => saleIds.Contains(x.SaleId)).ToListAsync(ct);
        Sales = sales.Select(sale => new CustomerSale(sale.Id, sale.Status, sale.FinalTotal ?? sale.Total, sale.CreatedAtUtc, sale.CompletedAtUtc, sale.Items.Select(x => $"{x.FrameBrand} {x.FrameModel} · {x.FrameColor}").ToArray(), orders.Where(x => x.SaleId == sale.Id).Select(x => new CustomerLaboratoryOrder(x.Status, x.ExpectedDeliveryDate)).ToArray())).ToArray();
        return Page();
    }
    public sealed record CustomerSale(Guid Id, SaleStatus Status, decimal Total, DateTimeOffset CreatedAt, DateTimeOffset? CompletedAt, IReadOnlyList<string> Items, IReadOnlyList<CustomerLaboratoryOrder> LaboratoryOrders);
    public sealed record CustomerLaboratoryOrder(LaboratoryOrderStatus Status, DateOnly? ExpectedDate);
}
