using _3D_viewer.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Org.BouncyCastle.Asn1.Mozilla;
using System;

namespace _3D_viewer.Data
{
    public class App_Db_Context : DbContext
    {
        public DbSet<Triangles> triangles { get; set; }
        public DbSet<Faces> faces { get; set; }

        public App_Db_Context(DbContextOptions<App_Db_Context> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Triangles>().HasKey(t => t.id);

            modelBuilder.Entity<Faces>().HasKey(f => new { f.triangle1_id, f.triangle2_id, f.triangle3_id });

            modelBuilder.Entity<Faces>()
                .HasOne(f => f.triangle1)
                .WithMany(t => t.faces1)
                .HasForeignKey(f => f.triangle1_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Faces>()
                .HasOne(f => f.triangle2)
                .WithMany(t => t.faces2)
                .HasForeignKey(f => f.triangle2_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Faces>()
                .HasOne(f => f.triangle3)
                .WithMany(t => t.faces3)
                .HasForeignKey(f => f.triangle3_id)
                .OnDelete(DeleteBehavior.Cascade);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql("server=localhost;database=model;user=root;password=Zn+H2SO4",
                    new MySqlServerVersion(new Version(8, 0, 30)));
            }
        }


    }
}
