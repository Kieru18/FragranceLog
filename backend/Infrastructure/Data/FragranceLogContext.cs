using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class FragranceLogContext : DbContext
{
    public FragranceLogContext()
    {
    }

    public FragranceLogContext(DbContextOptions<FragranceLogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Daytime> Daytimes { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Group> Groups { get; set; }

    public virtual DbSet<Longevity> Longevities { get; set; }

    public virtual DbSet<Note> Notes { get; set; }

    public virtual DbSet<NotePhoto> NotePhotos { get; set; }

    public virtual DbSet<NoteType> NoteTypes { get; set; }

    public virtual DbSet<Perfume> Perfumes { get; set; }

    public virtual DbSet<PerfumeDaytimeVote> PerfumeDaytimeVotes { get; set; }

    public virtual DbSet<PerfumeGenderVote> PerfumeGenderVotes { get; set; }

    public virtual DbSet<PerfumeLongevityVote> PerfumeLongevityVotes { get; set; }

    public virtual DbSet<PerfumeNote> PerfumeNotes { get; set; }

    public virtual DbSet<PerfumePhoto> PerfumePhotos { get; set; }

    public virtual DbSet<PerfumeSeasonVote> PerfumeSeasonVotes { get; set; }

    public virtual DbSet<PerfumeSillageVote> PerfumeSillageVotes { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<ReviewPhoto> ReviewPhotos { get; set; }

    public virtual DbSet<Season> Seasons { get; set; }

    public virtual DbSet<Sillage> Sillages { get; set; }

    public virtual DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AI");

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasOne(d => d.Company).WithMany(p => p.Brands)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Brands_Company");
        });

        modelBuilder.Entity<Daytime>(entity =>
        {
            entity.Property(e => e.DaytimeId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.Property(e => e.GenderId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Longevity>(entity =>
        {
            entity.Property(e => e.LongevityId).ValueGeneratedNever();
        });

        modelBuilder.Entity<NotePhoto>(entity =>
        {
            entity.Property(e => e.UploadDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Note).WithOne(p => p.NotePhoto).HasConstraintName("FK_NotePhotos_Note");
        });

        modelBuilder.Entity<NoteType>(entity =>
        {
            entity.Property(e => e.NoteTypeId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Perfume>(entity =>
        {
            entity.HasOne(d => d.Brand).WithMany(p => p.Perfumes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Perfumes_Brand");

            entity.HasOne(d => d.CountryCodeNavigation).WithMany(p => p.Perfumes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Perfumes_Country");

            entity.HasMany(d => d.Groups).WithMany(p => p.Perfumes)
                .UsingEntity<Dictionary<string, object>>(
                    "PerfumeGroup",
                    r => r.HasOne<Group>().WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PerfumeGroup_Group"),
                    l => l.HasOne<Perfume>().WithMany()
                        .HasForeignKey("PerfumeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PerfumeGroup_Perfume"),
                    j =>
                    {
                        j.HasKey("PerfumeId", "GroupId");
                        j.ToTable("PerfumeGroup");
                    });
        });

        modelBuilder.Entity<PerfumeDaytimeVote>(entity =>
        {
            entity.HasKey(e => new { e.PerfumeId, e.UserId }).HasName("PK_DaytimeVotes");

            entity.Property(e => e.VoteDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Daytime).WithMany(p => p.PerfumeDaytimeVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DaytimeVotes_Daytime");

            entity.HasOne(d => d.Perfume).WithMany(p => p.PerfumeDaytimeVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DaytimeVotes_Perfume");

            entity.HasOne(d => d.User).WithMany(p => p.PerfumeDaytimeVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DaytimeVotes_User");
        });

        modelBuilder.Entity<PerfumeGenderVote>(entity =>
        {
            entity.HasKey(e => new { e.PerfumeId, e.UserId }).HasName("PK_GenderVotes");

            entity.Property(e => e.VoteDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Gender).WithMany(p => p.PerfumeGenderVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GenderVotes_Gender");

            entity.HasOne(d => d.Perfume).WithMany(p => p.PerfumeGenderVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GenderVotes_Perfume");

            entity.HasOne(d => d.User).WithMany(p => p.PerfumeGenderVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GenderVotes_User");
        });

        modelBuilder.Entity<PerfumeLongevityVote>(entity =>
        {
            entity.HasKey(e => new { e.PerfumeId, e.UserId }).HasName("PK_LongevityVotes");

            entity.Property(e => e.VoteDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Longevity).WithMany(p => p.PerfumeLongevityVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LongevityVotes_Longevity");

            entity.HasOne(d => d.Perfume).WithMany(p => p.PerfumeLongevityVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LongevityVotes_Perfume");

            entity.HasOne(d => d.User).WithMany(p => p.PerfumeLongevityVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_LongevityVotes_User");
        });

        modelBuilder.Entity<PerfumeNote>(entity =>
        {
            entity.HasOne(d => d.Note).WithMany(p => p.PerfumeNotes).HasConstraintName("FK_PerfumeNote_Note");

            entity.HasOne(d => d.NoteType).WithMany(p => p.PerfumeNotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PerfumeNote_NoteType");

            entity.HasOne(d => d.Perfume).WithMany(p => p.PerfumeNotes).HasConstraintName("FK_PerfumeNote_Perfume");
        });

        modelBuilder.Entity<PerfumePhoto>(entity =>
        {
            entity.Property(e => e.UploadDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Perfume).WithOne(p => p.PerfumePhoto).HasConstraintName("FK_PerfumePhotos_Perfume");
        });

        modelBuilder.Entity<PerfumeSeasonVote>(entity =>
        {
            entity.HasKey(e => new { e.PerfumeId, e.UserId }).HasName("PK_SeasonVotes");

            entity.Property(e => e.VoteDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Perfume).WithMany(p => p.PerfumeSeasonVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeasonVotes_Perfume");

            entity.HasOne(d => d.Season).WithMany(p => p.PerfumeSeasonVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeasonVotes_Season");

            entity.HasOne(d => d.User).WithMany(p => p.PerfumeSeasonVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SeasonVotes_User");
        });

        modelBuilder.Entity<PerfumeSillageVote>(entity =>
        {
            entity.HasKey(e => new { e.PerfumeId, e.UserId }).HasName("PK_SillageVotes");

            entity.Property(e => e.VoteDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Perfume).WithMany(p => p.PerfumeSillageVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SillageVotes_Perfume");

            entity.HasOne(d => d.Sillage).WithMany(p => p.PerfumeSillageVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SillageVotes_Sillage");

            entity.HasOne(d => d.User).WithMany(p => p.PerfumeSillageVotes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SillageVotes_User");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.Property(e => e.ReviewDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Perfume).WithMany(p => p.Reviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_Perfume");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reviews_User");
        });

        modelBuilder.Entity<ReviewPhoto>(entity =>
        {
            entity.Property(e => e.UploadDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Review).WithOne(p => p.ReviewPhoto)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_ReviewPhotos_Review");
        });

        modelBuilder.Entity<Season>(entity =>
        {
            entity.Property(e => e.SeasonId).ValueGeneratedNever();
        });

        modelBuilder.Entity<Sillage>(entity =>
        {
            entity.Property(e => e.SillageId).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(e => e.CreationDate).HasDefaultValueSql("(getdate())");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
