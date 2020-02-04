﻿// <auto-generated />
using System;
using Kitakun.VkModules.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Kitakun.VkModules.Persistance.Migrations
{
    [DbContext(typeof(VkDbContext))]
    partial class VkDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("Kitakun.VkModules.Core.Domain.DataCollection", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CalculationStartedAt");

                    b.Property<DateTime>("From");

                    b.Property<string>("JsonValue")
                        .HasColumnType("json");

                    b.Property<long>("OwnerExternalId");

                    b.Property<DateTime>("To");

                    b.Property<byte>("Type");

                    b.HasKey("Id");

                    b.ToTable("DataCollections");
                });

            modelBuilder.Entity("Kitakun.VkModules.Core.Domain.GroupSettings", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("BackgroundJobType");

                    b.Property<int>("CommentPrice");

                    b.Property<string>("GroupAppToken");

                    b.Property<long?>("GroupId");

                    b.Property<string>("LastRunnedJobId");

                    b.Property<int>("LikePrice");

                    b.Property<string>("RecuringBackgroundJobId");

                    b.Property<int>("RepostPrice");

                    b.Property<bool>("ReverseGroup");

                    b.Property<string>("TopLikersHeaderMessage");

                    b.HasKey("Id");

                    b.ToTable("GroupSettings");
                });

            modelBuilder.Entity("Kitakun.VkModules.Core.Domain.Subscription", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("From");

                    b.Property<long?>("GroupId");

                    b.Property<DateTime?>("To");

                    b.Property<long>("UserId");

                    b.HasKey("Id");

                    b.ToTable("Subscriptions");
                });
#pragma warning restore 612, 618
        }
    }
}
