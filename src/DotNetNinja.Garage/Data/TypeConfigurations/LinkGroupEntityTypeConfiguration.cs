using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetNinja.Garage.Data.TypeConfigurations;

public class LinkGroupEntityTypeConfiguration: IEntityTypeConfiguration<LinkGroup>
{
    public void Configure(EntityTypeBuilder<LinkGroup> builder)
    {
        builder.HasKey(e => e.Id).IsClustered().HasName("PK_LinkGroup");
        builder.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.LinkPageId).IsRequired();
        builder.Property(e => e.Text).IsRequired().HasMaxLength(64);
        builder.Property(e => e.SortIndex).IsRequired();
        builder.Property(e => e.Revision).IsConcurrencyToken().IsRequired();
        builder.HasIndex(e => new { e.LinkPageId, e.SortIndex });

        builder.HasMany(e => e.Links).WithOne(e => e.LinkGroup).HasForeignKey(e => e.LinkGroupId).OnDelete(DeleteBehavior.Cascade);
    }
}
