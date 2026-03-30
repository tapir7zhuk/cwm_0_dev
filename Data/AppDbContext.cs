using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vet_Master.Models;

namespace Vet_Master.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AnimalCard> AnimalCards => Set<AnimalCard>();
    public DbSet<Record> Records => Set<Record>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Record>()
            .HasOne(r => r.AnimalCard)
            .WithMany(a => a.Records)
            .HasForeignKey(r => r.AnimalCardId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Vaccination>()
            .HasOne(v => v.AnimalCard)
            .WithMany(a => a.Vaccinations)
            .HasForeignKey(v => v.AnimalCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}