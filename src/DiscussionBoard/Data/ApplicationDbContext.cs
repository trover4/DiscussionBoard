using DiscussionBoard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<DiscussionThread> DiscussionThreads { get; set; } = null!;

        public DbSet<Forum> Forums { get; set; } = null!;

        public DbSet<Post> Posts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DiscussionThread>()
                .HasOne(dt => dt.Forum)
                .WithMany(f => f.DiscussionThreads)
                .HasForeignKey(dt => dt.ForumId);

            builder.Entity<Post>()
                .HasOne(p => p.DiscussionThread)
                .WithMany(dt => dt.Posts)
                .HasForeignKey(p => p.DiscussionThreadId);

            builder.Entity<Post>()
                .HasOne(p => p.ParentPost)
                .WithMany(p => p.Replies)
                .HasForeignKey(p => p.ParentPostId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<DiscussionThread>()
                .HasOne(dt => dt.ApplicationUser)
                .WithMany(u => u.DiscussionThreads)
                .HasForeignKey(dt => dt.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Post>()
                .HasOne(p => p.ApplicationUser)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
