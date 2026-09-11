namespace OticaVisao.Infrastructure.Authentication;

public static class AdminAuthorization
{
    public const string Policy = "AdminOnly";

    public const string Role = "Administrator";

    public const string GeneralRole = "GeneralAdministrator";

    public const string GeneralPolicy = "GeneralAdminOnly";

    public const string DisplayNameClaim = "display_name";

    public const string PermissionClaim = "admin_permission";

    public const string AllPanelsPermission = "all_panels";
    public const string FramesPermission = "frames";
    public const string CustomersPermission = "customers";
    public const string SalesPermission = "sales";
    public const string ReservationsPermission = "reservations";
    public const string LaboratoryOrdersPermission = "laboratory_orders";
    public const string ReportsPermission = "reports";
    public const string AuditPermission = "audit";

    public const string FramesPolicy = "AdminFrames";
    public const string CustomersPolicy = "AdminCustomers";
    public const string SalesPolicy = "AdminSales";
    public const string ReservationsPolicy = "AdminReservations";
    public const string LaboratoryOrdersPolicy = "AdminLaboratoryOrders";
    public const string ReportsPolicy = "AdminReports";
    public const string AuditPolicy = "AdminAudit";

    public static readonly IReadOnlyList<AdminPermissionDefinition> Permissions =
    [
        new(FramesPermission, FramesPolicy, "Armações e estoque"),
        new(CustomersPermission, CustomersPolicy, "Clientes"),
        new(SalesPermission, SalesPolicy, "Vendas"),
        new(ReservationsPermission, ReservationsPolicy, "Reservas"),
        new(LaboratoryOrdersPermission, LaboratoryOrdersPolicy, "Pedidos de lentes"),
        new(ReportsPermission, ReportsPolicy, "Relatórios"),
        new(AuditPermission, AuditPolicy, "Auditoria e histórico")
    ];

    public static bool HasPermission(System.Security.Claims.ClaimsPrincipal user, string permission) =>
        user.IsInRole(GeneralRole)
        || user.HasClaim(PermissionClaim, AllPanelsPermission)
        || user.HasClaim(PermissionClaim, permission);

    public static string GetPanelLandingPage(System.Security.Claims.ClaimsPrincipal user)
    {
        if (HasPermission(user, FramesPermission)) return "/Admin/Frames/Index";
        if (HasPermission(user, CustomersPermission)) return "/Admin/Customers/Index";
        if (HasPermission(user, SalesPermission)) return "/Admin/Sales/Index";
        if (HasPermission(user, ReservationsPermission)) return "/Admin/Reservations/Index";
        if (HasPermission(user, LaboratoryOrdersPermission)) return "/Admin/LaboratoryOrders/Index";
        if (HasPermission(user, ReportsPermission)) return "/Admin/Reports/Index";
        if (HasPermission(user, AuditPermission)) return "/Admin/Audit/Index";
        return "/Admin/Account/AccessDenied";
    }
}

public sealed record AdminPermissionDefinition(string Value, string Policy, string Label);
