using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Kitakun.VkModules.Core.Domain;

namespace Kitakun.VkModules.Persistance.EntityConfigurations
{
	public class SubscriptionEntityConfiguration : IEntityTypeConfiguration<Subscription>
	{
		public void Configure(EntityTypeBuilder<Subscription> builder)
		{
			builder.ToTable("Subscriptions");

			builder.HasKey(x => x.Id);

			builder.Property(e => e.From);
			builder.Property(e => e.To);
			builder.Property(e => e.GroupId);
			builder.Property(e => e.UserId);
		}
	}
}
