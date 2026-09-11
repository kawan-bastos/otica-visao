using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OticaVisao.Infrastructure.Authentication;

namespace OticaVisao.Infrastructure.Persistence.Configurations;

public sealed class PasswordHistoryEntryConfiguration : IEntityTypeConfiguration<PasswordHistoryEntry>
{
    public void Configure(EntityTypeBuilder<PasswordHistoryEntry> builder)
    {
        builder.ToTable("PasswordHistory");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.PasswordHash).HasMaxLength(1000).IsRequired();
        builder.HasIndex(entry => new { entry.UserId, entry.ChangedAtUtc });
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(entry => entry.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
