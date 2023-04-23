using System;
using _3D_viewer.models;
using Microsoft.EntityFrameworkCore;

namespace _3D_viewer
{
    public class App_Db_Context : DbContext
    {
        public DbSet<Vertices> vertices { get; set; }
        public DbSet<Faces> faces { get; set; }

        public App_Db_Context()
        {
        }

        public App_Db_Context(DbContextOptions<App_Db_Context> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vertices>().HasKey(t => t.id);

            modelBuilder.Entity<Faces>().HasKey(f => new { triangle1_id = f.vertex1_id, triangle2_id = f.vertex2_id, triangle3_id = f.vertex3_id });

            modelBuilder.Entity<Faces>()
                .HasOne(f => f.vertex1)
                .WithMany(t => t.faces1)
                .HasForeignKey(f => f.vertex1_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Faces>()
                .HasOne(f => f.vertex2)
                .WithMany(t => t.faces2)
                .HasForeignKey(f => f.vertex2_id)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Faces>()
                .HasOne(f => f.vertex3)
                .WithMany(t => t.faces3)
                .HasForeignKey(f => f.vertex3_id)
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