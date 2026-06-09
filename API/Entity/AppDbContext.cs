using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<Content> Contents { get; set; }
    public DbSet<Genres> Genres { get; set; }
    public DbSet<ContentGenres> ContentGenres { get; set; }
    public DbSet<Users> Users { get; set; }
    public DbSet<UserPreferedGenres> UserPreferedGenres { get; set; }
    public DbSet<UserInteractions> UserInteractions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Content>(entity =>
        {
            entity.ToTable("contents");
            entity.HasKey(c => c.ShowId);
            entity.Property(c => c.ShowId).HasColumnName("show_id").HasMaxLength(50);
            entity.Property(c => c.Title).HasColumnName("title").HasMaxLength(255).IsRequired();
            entity.Property(c => c.ContentType).HasColumnName("type").HasMaxLength(20); // Khớp cột 'type'
            entity.Property(c => c.ClusterId).HasColumnName("cluster_id"); // Khớp cột 'cluster_id'
        });

        modelBuilder.Entity<Genres>(entity =>
        {
            entity.ToTable("genres");
            entity.HasKey(g => g.GenreId);
            entity.Property(g => g.GenreId).HasColumnName("genre_id");
            entity.Property(g => g.GenreName).HasColumnName("genre_name").HasMaxLength(100).IsRequired();
            entity.HasIndex(g => g.GenreName).IsUnique(); // Đảm bảo tên thể loại là duy nhất
        });

        modelBuilder.Entity<ContentGenres>(entity =>
        {
            entity.ToTable("content_genre"); // Đổi từ ContentGenres thành content_genre
            entity.HasKey(cg => new { cg.ShowId, cg.GenreId });

            // Map thuộc tính showid trong C# thành cột show_id dưới DB để khớp với Python
            entity.Property(cg => cg.ShowId).HasColumnName("show_id").HasMaxLength(50);
            entity.Property(cg => cg.GenreId).HasColumnName("genre_id");
        });

        modelBuilder.Entity<Users>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.UserId);
            entity.Property(u => u.UserId).HasColumnName("user_id");
            entity.Property(u => u.UserName).HasColumnName("username").HasMaxLength(100).IsRequired();
            entity.Property(u => u.Password).HasColumnName("password").HasMaxLength(100);
            entity.HasIndex(u => u.UserName).IsUnique();
        });

        modelBuilder.Entity<UserPreferedGenres>(entity =>
        {
            entity.ToTable("user_preferred_genres");
            entity.HasKey(upg => new { upg.UserId, upg.GenreId });
            entity.Property(upg => upg.UserId).HasColumnName("user_id");
            entity.Property(upg => upg.GenreId).HasColumnName("genre_id");
        });

        modelBuilder.Entity<UserInteractions>(entity =>
        {
            entity.ToTable("user_interactions");
            entity.HasKey(ui => ui.InteractionId);
            entity.Property(ui => ui.InteractionId).HasColumnName("interaction_id");
            entity.Property(ui => ui.UserId).HasColumnName("user_id");
            entity.Property(ui => ui.ShowId).HasColumnName("show_id").HasMaxLength(50); // Map ShowId thành show_id
            entity.Property(ui => ui.Liked).HasColumnName("is_liked"); // Map Liked (bool) sang is_liked (bit)
            entity.Property(ui => ui.Timestamp)
                  .HasColumnName("interacted_at")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Khớp cấu hình thời gian mặc định
        });
    }
}