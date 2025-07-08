using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TDEV_811.Models;

public partial class Tdev811Context : DbContext
{
    public Tdev811Context(DbContextOptions<Tdev811Context> options)
        : base(options)
    {
    }

    public virtual DbSet<MqttMessage> MqttMessages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MqttMessage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("mqtt_messages_pkey");

            entity.ToTable("mqtt_messages");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Payload)
                .HasColumnType("character varying")
                .HasColumnName("payload");
            entity.Property(e => e.Topic)
                .HasColumnType("character varying")
                .HasColumnName("topic");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username)
                .HasColumnType("character varying")
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
