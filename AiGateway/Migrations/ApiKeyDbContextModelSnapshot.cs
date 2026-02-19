using System;
using AiGateway.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AiGateway.Migrations;

[DbContext(typeof(ApiKeyDbContext))]
public partial class ApiKeyDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity("AiGateway.Data.ClientKeyEntity", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedNever()
                .HasColumnType("TEXT");

            b.Property<string>("AppContact")
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("TEXT");

            b.Property<string>("AppName")
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnType("TEXT");

            b.Property<string>("AppNote")
                .HasMaxLength(1000)
                .HasColumnType("TEXT");

            b.Property<DateTime>("CreatedAtUtc")
                .HasColumnType("TEXT");

            b.Property<bool>("Enabled")
                .HasColumnType("INTEGER");

            b.Property<byte[]>("KeyHash")
                .IsRequired()
                .HasColumnType("BLOB");

            b.Property<DateTime?>("LastUsedAtUtc")
                .HasColumnType("TEXT");

            b.Property<byte[]>("Salt")
                .IsRequired()
                .HasColumnType("BLOB");

            b.Property<DateTime>("UpdatedAtUtc")
                .HasColumnType("TEXT");

            b.HasKey("Id");

            b.ToTable("ClientKeys");
        });
    }
}
