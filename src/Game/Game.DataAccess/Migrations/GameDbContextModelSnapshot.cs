﻿// <auto-generated />
using Game.DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Game.DataAccess.Migrations
{
    [DbContext(typeof(GameDbContext))]
    partial class GameDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.22");

            modelBuilder.Entity("Game.DataAccess.Context.Entities.Gift", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReceiverPlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ResourceTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("ResourceValue")
                        .HasColumnType("REAL");

                    b.Property<int>("SenderPlayerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ReceiverPlayerId");

                    b.HasIndex("ResourceTypeId");

                    b.HasIndex("SenderPlayerId");

                    b.ToTable("Gifts");
                });

            modelBuilder.Entity("Game.DataAccess.Context.Entities.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("DeviceId")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DeviceId")
                        .IsUnique();

                    b.ToTable("Players");
                });

            modelBuilder.Entity("Game.DataAccess.Context.Entities.Resource", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ResourceTypeId")
                        .HasColumnType("INTEGER");

                    b.Property<double>("ResourceValue")
                        .IsConcurrencyToken()
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("ResourceTypeId");

                    b.HasIndex("PlayerId", "ResourceTypeId")
                        .IsUnique();

                    b.ToTable("Resources");

                    b.HasCheckConstraint("CK_Resource_ResourceValue", "ResourceValue >= 0");
                });

            modelBuilder.Entity("Game.DataAccess.Context.Entities.ResourceType", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ResourceTypeName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("ResourceTypes");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            ResourceTypeName = "Coins"
                        },
                        new
                        {
                            Id = 2,
                            ResourceTypeName = "Rolls"
                        });
                });

            modelBuilder.Entity("Game.DataAccess.Context.Entities.Gift", b =>
                {
                    b.HasOne("Game.DataAccess.Context.Entities.Player", "Receiver")
                        .WithMany("ReceivedGifts")
                        .HasForeignKey("ReceiverPlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Game.DataAccess.Context.Entities.ResourceType", "ResourceType")
                        .WithMany()
                        .HasForeignKey("ResourceTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Game.DataAccess.Context.Entities.Player", "Sender")
                        .WithMany("SentGifts")
                        .HasForeignKey("SenderPlayerId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Receiver");

                    b.Navigation("ResourceType");

                    b.Navigation("Sender");
                });

            modelBuilder.Entity("Game.DataAccess.Context.Entities.Resource", b =>
                {
                    b.HasOne("Game.DataAccess.Context.Entities.Player", "Player")
                        .WithMany("Resources")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Game.DataAccess.Context.Entities.ResourceType", "ResourceType")
                        .WithMany("Resources")
                        .HasForeignKey("ResourceTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");

                    b.Navigation("ResourceType");
                });

            modelBuilder.Entity("Game.DataAccess.Context.Entities.Player", b =>
                {
                    b.Navigation("ReceivedGifts");

                    b.Navigation("Resources");

                    b.Navigation("SentGifts");
                });

            modelBuilder.Entity("Game.DataAccess.Context.Entities.ResourceType", b =>
                {
                    b.Navigation("Resources");
                });
#pragma warning restore 612, 618
        }
    }
}
