﻿using System;
using _3D_viewer.models;
using Microsoft.EntityFrameworkCore;

namespace _3D_viewer;

public class App_Db_Context : DbContext
{
    public App_Db_Context()
    {
    }

    public App_Db_Context(DbContextOptions<App_Db_Context> options) : base(options)
    {
    }

    public DbSet<Vertices> vertices { get; set; }
    public DbSet<Faces> faces { get; set; }
    public DbSet<Objects> objects { get; set; }

    protected override void OnModelCreating(ModelBuilder model_builder)
    {
        model_builder.Entity<Vertices>().HasKey(t => t.id);
        
        model_builder.Entity<Objects>().HasKey(t => t.object_id);

        model_builder.Entity<Faces>().HasKey(f => new
            { triangle1_id = f.vertex1_id, triangle2_id = f.vertex2_id, triangle3_id = f.vertex3_id });

        model_builder.Entity<Faces>()
            .HasOne(f => f.vertex1)
            .WithMany(t => t.faces1)
            .HasForeignKey(f => f.vertex1_id)
            .OnDelete(DeleteBehavior.Cascade);

        model_builder.Entity<Faces>()
            .HasOne(f => f.vertex2)
            .WithMany(t => t.faces2)
            .HasForeignKey(f => f.vertex2_id)
            .OnDelete(DeleteBehavior.Cascade);

        model_builder.Entity<Faces>()
            .HasOne(f => f.vertex3)
            .WithMany(t => t.faces3)
            .HasForeignKey(f => f.vertex3_id)
            .OnDelete(DeleteBehavior.Cascade);

        model_builder.Entity<Faces>()
            .HasOne(f => f.objects)
            .WithMany(o => o.faces)
            .HasForeignKey(f => f.object_id)
            .OnDelete(DeleteBehavior.Cascade);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options_builder)
    {
        if (!options_builder.IsConfigured)
            options_builder.UseMySql("server=localhost;database=model;user=root;password=Zn+H2SO4",
                new MySqlServerVersion(new Version(8, 0, 30)));
    }
}