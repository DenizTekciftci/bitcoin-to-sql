using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace Btc.Database.Dto;

public partial class BitcoinContext : DbContext
{
    public BitcoinContext()
    {
    }

    public BitcoinContext(DbContextOptions<BitcoinContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Block> Blocks { get; set; }

    public virtual DbSet<Input> Inputs { get; set; }

    public virtual DbSet<Output> Outputs { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Block>(entity =>
        {
            entity.HasKey(e => e.Hash).HasName("PRIMARY");

            entity.ToTable("blocks");

            entity.Property(e => e.Hash)
                .HasMaxLength(64)
                .HasColumnName("hash");
            entity.Property(e => e.File).HasColumnName("file");
            entity.Property(e => e.Height).HasColumnName("height");
            entity.Property(e => e.PrevHash)
                .HasMaxLength(64)
                .HasColumnName("prev_hash");
            entity.Property(e => e.Time)
                .HasColumnType("datetime")
                .HasColumnName("time");
        });

        modelBuilder.Entity<Input>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("inputs");

            entity.HasIndex(e => e.Txid, "input_index");

            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Source)
                .HasMaxLength(64)
                .HasColumnName("source");
            entity.Property(e => e.Txid)
                .HasMaxLength(64)
                .HasColumnName("txid");
        });

        modelBuilder.Entity<Output>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("outputs");

            entity.HasIndex(e => e.Txid, "output_index");

            entity.Property(e => e.Address)
                .HasMaxLength(100)
                .HasColumnName("address");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Txid)
                .HasMaxLength(64)
                .HasColumnName("txid");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("transactions");

            entity.HasIndex(e => e.Txid, "txid_UNIQUE").IsUnique();

            entity.Property(e => e.Block)
                .HasMaxLength(64)
                .HasColumnName("block");
            entity.Property(e => e.File).HasColumnName("file");
            entity.Property(e => e.Txid)
                .HasMaxLength(64)
                .HasColumnName("txid");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
