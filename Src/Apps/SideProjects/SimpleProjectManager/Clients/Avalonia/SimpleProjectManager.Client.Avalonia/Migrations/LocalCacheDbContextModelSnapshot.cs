﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SimpleProjectManager.Client.Avalonia.Models.Data;

#nullable disable

namespace SimpleProjectManager.Client.Avalonia.Migrations
{
    [DbContext(typeof(LocalCacheDbContext))]
    partial class LocalCacheDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("Tauron.Applicarion.Redux.Extensions.Cache.CacheData", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Data");
                });

            modelBuilder.Entity("Tauron.Applicarion.Redux.Extensions.Cache.CacheTimeout", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("DataKey")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("Timeout")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Timeout");
                });
#pragma warning restore 612, 618
        }
    }
}
