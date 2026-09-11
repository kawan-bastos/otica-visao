using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Domain.Costs;
namespace OticaVisao.Infrastructure.Persistence.Configurations;
internal sealed class BusinessCostConfiguration : IEntityTypeConfiguration<BusinessCost>
{
    public void Configure(EntityTypeBuilder<BusinessCost> b) { b.ToTable("business_costs"); b.HasKey(x => x.Id); b.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever(); b.Property(x => x.Category).HasColumnName("category").HasConversion<string>().HasMaxLength(30); b.Property(x => x.Amount).HasColumnName("amount").HasPrecision(12,2); b.Property(x => x.IncurredOn).HasColumnName("incurred_on"); b.Property(x => x.Description).HasColumnName("description").HasMaxLength(200); b.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc"); b.HasIndex(x => x.IncurredOn).HasDatabaseName("ix_business_costs_incurred_on"); }
}
