using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Costs;
namespace OticaVisao.Infrastructure.Persistence.Configurations;
internal sealed class MonthlyCostBudgetConfiguration : IEntityTypeConfiguration<MonthlyCostBudget>
{
    public void Configure(EntityTypeBuilder<MonthlyCostBudget> b) { b.ToTable("monthly_cost_budgets"); b.HasKey(x => x.Id); b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever(); b.Property(x => x.Month).HasColumnName("month"); b.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12,2); b.HasIndex(x => x.Month).IsUnique().HasDatabaseName("ix_monthly_cost_budgets_month"); }
}
