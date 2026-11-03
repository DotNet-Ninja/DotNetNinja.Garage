using DotNetNinja.Garage.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DotNetNinja.Garage.Data.TypeConfigurations;

public class LinkItemEntityTypeConfiguration: IEntityTypeConfiguration<LinkItem>
{
    public void Configure(EntityTypeBuilder<LinkItem> builder)
    {
        builder.HasKey(e => e.Id).IsClustered().HasName("PK_LinkItem");
        builder.Property(e => e.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Property(e => e.LinkGroupId).IsRequired();
        builder.Property(e => e.Href).IsRequired().HasMaxLength(256);
        builder.Property(e=>e.Icon).HasMaxLength(64).HasConversion<EnumToStringConverter<Icons>>().IsRequired();
        builder.Property(e=>e.IconColor).HasMaxLength(16).IsRequired();
        builder.Property(e=>e.OpenInNewTab).IsRequired();
        builder.Property(e => e.Text).IsRequired().HasMaxLength(64);
        builder.Property(e => e.SortIndex).IsRequired();
    }
}