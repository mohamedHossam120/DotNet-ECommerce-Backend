using E_Commerce.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace E_Commerce.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.ShippingAddressLine1)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(o => o.ShippingAddressLine2)
            .HasMaxLength(250);

        builder.Property(o => o.ShippingCity)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ShippingCountry)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.ShippingPostalCode)
            .HasMaxLength(20);

        builder.Property(o => o.ShippingPhoneNumber)
            .HasMaxLength(20);

        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(o => o.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(o => o.CreatedAtUtc)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(o => o.UserId);

        builder.HasIndex(o => o.Status);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}