using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetNinja.Garage.Data.TypeConfigurations;

public class LinkPageEntityTypeConfiguration: IEntityTypeConfiguration<LinkPage>
{
    public void Configure(EntityTypeBuilder<LinkPage> builder)
    {
        builder.HasKey(e=>e.Id).IsClustered().HasName("PK_LinkPage");
        builder.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e=>e.UserId).IsRequired().HasMaxLength(256);
        builder.Property(e=>e.Slug).IsRequired().HasMaxLength(128);
        builder.Property(e=>e.Text).IsRequired().HasMaxLength(64);
        builder.Property(e=>e.SortIndex).IsRequired();

        builder.HasMany(e => e.Groups).WithOne(e => e.LinkPage).HasForeignKey(e => e.LinkPageId).OnDelete(DeleteBehavior.Cascade);
    }
}
