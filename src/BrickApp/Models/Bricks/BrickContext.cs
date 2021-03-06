﻿using System;
using System.Linq;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;

namespace BrickApp.Models.Bricks
{
    public class BrickContext : DbContext
    {
        public BrickContext(DbContextOptions<BrickContext> options)
            : base(options)
        { }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        public DbSet<Gallery> Galleries { get; set; }
        public DbSet<GalleryContent> GalleryContent { get; set; }
        public DbSet<Redirect> Redirects { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<Menu> Menus { get; set; }

        public override int SaveChanges()
        {
            this.ChangeTracker.DetectChanges();

            var posts = this.ChangeTracker.Entries<Post>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var post in posts)
            {
                if (post.State == EntityState.Added)
                    post.Property("DateCreated").CurrentValue = DateTime.UtcNow;                                   

                post.Property("LastUpdated").CurrentValue = DateTime.UtcNow;
            }

            return base.SaveChanges();
        }
    }
}