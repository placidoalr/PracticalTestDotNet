using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PracticalTestDotNet.Domain.Entities;

namespace PracticalTestDotNet.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        // Ignore computed TotalAmount property in EF Core
        builder.Ignore(o => o.TotalAmount);

        // Configure backing field for Items collection
        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.OrderId)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.Property(i => i.UnitPrice)
            .HasColumnType("TEXT") // SQLite decimal storage format
            .IsRequired();
    }
}
