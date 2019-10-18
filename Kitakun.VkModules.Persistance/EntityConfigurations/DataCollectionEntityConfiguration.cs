using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Kitakun.VkModules.Core.Domain;

namespace Kitakun.VkModules.Persistance.EntityConfigurations
{
    public class DataCollectionEntityConfiguration : IEntityTypeConfiguration<DataCollection>
    {
        public void Configure(EntityTypeBuilder<DataCollection> builder)
        {
            builder.ToTable("DataCollections");

            builder.HasKey(x => x.Id);

            builder
                .Property(e => e.JsonValue)
                .HasColumnType("json");
        }
    }
}
