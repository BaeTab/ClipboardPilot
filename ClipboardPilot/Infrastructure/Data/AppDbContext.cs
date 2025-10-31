using Microsoft.EntityFrameworkCore;
using ClipboardPilot.Domain.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace ClipboardPilot.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<ClipboardItem> ClipboardItems { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ClipboardItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Hash);
            entity.HasIndex(e => e.Pinned);
            entity.HasIndex(e => e.FavoriteRank);
            entity.HasIndex(e => e.Type);
            entity.Property(e => e.Preview).HasMaxLength(500);
            entity.Property(e => e.SourceApp).HasMaxLength(256);
            entity.Property(e => e.Tags).HasMaxLength(1000);
            entity.Property(e => e.Hash).HasMaxLength(64);
        });
    }
}
