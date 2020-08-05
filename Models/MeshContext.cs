using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace MeshBackend.Models
{
    public class MeshContext : DbContext
    {
        public MeshContext(DbContextOptions<MeshContext> options) : base(options)
        {
        }

        public DbSet<User>Users { get; set; }
        
        public DbSet<Team>Teams { get; set; }
        
        public DbSet<Project>Projects { get; set; }
        
        public DbSet<Task>Tasks { get; set; }
        
        public DbSet<Subtask>Subtasks { get; set; }
        
        public DbSet<Assign>Assigns { get; set; }
        
        public DbSet<Bulletin>Bulletins { get; set; }
        
        public DbSet<BulletinBoard>BulletinBoards { get; set; }
        
        public DbSet<Cooperation>Cooperations { get; set; }
        
        public DbSet<TaskFeed>TaskFeeds { get; set; }
        
        public DbSet<TaskBoard>TaskBoards { get; set; }
        
        public DbSet<TaskTag>TaskTags { get; set; }
        
        public DbSet<BulletinFeed>BulletinFeeds { get; set; }
        
        public DbSet<ProjectMemoCollection>ProjectMemoCollections { get; set; }
        
        public DbSet<ProjectMemo>ProjectMemos { get; set; }
        
        public DbSet<TeamMemoCollection>TeamMemoCollections { get; set; }
        
        public DbSet<TeamMemo>TeamMemos { get; set; }
        
        public DbSet<Admin>Admins { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            //TaskTag
            modelBuilder.Entity<TaskTag>()
                .HasKey(c => new {c.TaskId, c.tag});
            modelBuilder.Entity<TaskTag>()
                .HasOne(c => c.Task)
                .WithMany(b => b.TaskTags)
                .HasForeignKey(c => c.TaskId);
            modelBuilder.Entity<TaskTag>()
                .Property(b => b.tag)
                .HasMaxLength(20);

            //Subtask
            modelBuilder.Entity<Subtask>()
                .HasKey(c => new {c.TaskId, c.Title});
            modelBuilder.Entity<Subtask>()
                .HasOne(c => c.Task)
                .WithMany(b => b.Subtasks)
                .HasForeignKey(c => c.TaskId);
            modelBuilder.Entity<Subtask>()
                .Property(b => b.Finished)
                .HasDefaultValue(false);
            modelBuilder.Entity<Subtask>()
                .Property(b => b.Title)
                .HasMaxLength(50);
            

            //Cooperation
            modelBuilder.Entity<Cooperation>()
                .HasKey(c => new {c.UserId, c.TeamId});
            modelBuilder.Entity<Cooperation>()
                .HasOne(t => t.Team)
                .WithMany(c => c.Cooperations)
                .HasForeignKey(t => t.TeamId);
            modelBuilder.Entity<Cooperation>()
                .HasOne(t => t.User)
                .WithMany(c => c.Cooperations)
                .HasForeignKey(t => t.UserId);
            
            //Assign
            modelBuilder.Entity<Assign>()
                .HasKey(c => new {c.TaskId, c.Title, c.UserId});
            modelBuilder.Entity<Assign>()
                .HasOne(t => t.Task)
                .WithMany(c => c.Assigns)
                .HasForeignKey(t => t.TaskId);
            modelBuilder.Entity<Assign>()
                .HasOne(t => t.User)
                .WithMany(c => c.Assigns)
                .HasForeignKey(c => c.UserId);
            modelBuilder.Entity<Assign>()
                .Property(b => b.Title)
                .HasMaxLength(50);

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}