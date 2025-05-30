using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TyutyunnikovaAnna_Diplom.Models;

namespace TyutyunnikovaAnna_Diplom.Context;

public partial class DiplomHorseClubContext : DbContext
{
    public DiplomHorseClubContext()
    {
    }

    public DiplomHorseClubContext(DbContextOptions<DiplomHorseClubContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BreederTraining> BreederTrainings { get; set; }

    public virtual DbSet<BreederTrainingType> BreederTrainingTypes { get; set; }

    public virtual DbSet<Clubnews> Clubnews { get; set; }

    public virtual DbSet<Competition> Competitions { get; set; }

    public virtual DbSet<Horse> Horses { get; set; }

    public virtual DbSet<HorseStable> HorseStables { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Stable> Stables { get; set; }

    public virtual DbSet<Stabletype> Stabletypes { get; set; }

    public virtual DbSet<TrainingType> TrainingTypes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserHorse> UserHorses { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Port=5432;Host=localhost;Username=postgres;Database=Diplom_HorseClub;Password=6274");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BreederTraining>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("breeder_trainings_pkey");

            entity.ToTable("breeder_trainings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Breedertrainingtypeid).HasColumnName("breedertrainingtypeid");

            // Изменено на timestamp without time zone
            entity.Property(e => e.Enddate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("enddate");

            entity.Property(e => e.Horseid).HasColumnName("horseid");

            entity.Property(e => e.IsNotificationSent).HasColumnName("isnotificationsent");

            // Изменено на timestamp without time zone
            entity.Property(e => e.Startdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("startdate");

            entity.Property(e => e.Status)
                .HasColumnType("character varying")
                .HasColumnName("status");
            entity.Property(e => e.description)
                .HasColumnType("character varying")
                .HasColumnName("description");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.Property(e => e.Cost)
                .HasPrecision(10, 2)
                .HasColumnName("cost");

            entity.HasOne(d => d.Breedertrainingtype).WithMany(p => p.BreederTrainings)
                .HasForeignKey(d => d.Breedertrainingtypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("breeder_trainings_breedertrainingtypeid_fkey");

            entity.HasOne(d => d.Horse).WithMany(p => p.BreederTrainings)
                .HasForeignKey(d => d.Horseid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("breeder_trainings_horseid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.BreederTrainings)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("breeder_trainings_userid_fkey");
        });

        modelBuilder.Entity<BreederTrainingType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("breeder_training_types_pkey");

            entity.ToTable("breeder_training_types");

            entity.HasIndex(e => new { e.Breederid, e.Trainingtypeid }, "breeder_training_types_breederid_trainingtypeid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Breederid).HasColumnName("breederid");
            entity.Property(e => e.Costoverride)
                .HasPrecision(10, 2)
                .HasColumnName("costoverride");
            entity.Property(e => e.Trainingtypeid).HasColumnName("trainingtypeid");

            entity.HasOne(d => d.Breeder).WithMany(p => p.BreederTrainingTypes)
                .HasForeignKey(d => d.Breederid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("breeder_training_types_breederid_fkey");

            entity.HasOne(d => d.Trainingtype).WithMany(p => p.BreederTrainingTypes)
                .HasForeignKey(d => d.Trainingtypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("breeder_training_types_trainingtypeid_fkey");
        });

        modelBuilder.Entity<Clubnews>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clubnews_pkey");

            entity.ToTable("clubnews");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Author)
                .HasMaxLength(100)
                .HasColumnName("author");
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Photo)
                .HasColumnType("character varying")
                .HasColumnName("photo");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Competition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("competition_pkey");

            entity.ToTable("competition");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Competitiontype)
                .HasMaxLength(100)
                .HasColumnName("competitiontype");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Entryfee)
                .HasMaxLength(50)
                .HasColumnName("entryfee");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Photocomp).HasColumnName("photocomp");
            entity.Property(e => e.Route)
                .HasMaxLength(255)
                .HasColumnName("route");
            entity.Property(e => e.Winnerid).HasColumnName("winnerid");

            entity.HasOne(d => d.Winner).WithMany(p => p.Competitions)
                .HasForeignKey(d => d.Winnerid)
                .HasConstraintName("competition_winnerid_fkey");
        });

        modelBuilder.Entity<Horse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("horses_pkey");

            entity.ToTable("horses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Breed)
                .HasMaxLength(100)
                .HasColumnName("breed");
            entity.Property(e => e.Datebirth)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datebirth");
            entity.Property(e => e.Gender)
                .HasMaxLength(100)
                .HasColumnName("gender");
            entity.Property(e => e.HorseName)
                .HasMaxLength(100)
                .HasColumnName("horse_name");
            entity.Property(e => e.HorsePhoto)
                .HasColumnType("character varying")
                .HasColumnName("horse_photo");
            entity.Property(e => e.VetPasport).HasColumnName("vet_pasport");
        });

        modelBuilder.Entity<HorseStable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("horse_stable_pkey");

            entity.ToTable("horse_stable");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Assignmentdate).HasColumnName("assignmentdate");
            entity.Property(e => e.Horseid).HasColumnName("horseid");
            entity.Property(e => e.Stableid).HasColumnName("stableid");

            entity.HasOne(d => d.Horse).WithMany(p => p.HorseStables)
                .HasForeignKey(d => d.Horseid)
                .HasConstraintName("horse_stable_horseid_fkey");

            entity.HasOne(d => d.Stable).WithMany(p => p.HorseStables)
                .HasForeignKey(d => d.Stableid)
                .HasConstraintName("horse_stable_stableid_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Stable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stables_pkey");

            entity.ToTable("stables");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StableCode)
                .HasMaxLength(100)
                .HasColumnName("stable_code");
            entity.Property(e => e.Typeid).HasColumnName("typeid");

            entity.HasOne(d => d.Type).WithMany(p => p.Stables)
                .HasForeignKey(d => d.Typeid)
                .HasConstraintName("stables_typeid_fkey");
        });

        modelBuilder.Entity<Stabletype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stabletypes_pkey");

            entity.ToTable("stabletypes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StablePhoto)
                .HasColumnType("character varying")
                .HasColumnName("stable_photo");
            entity.Property(e => e.StablesArendable).HasColumnName("stables_arendable");
        });

        modelBuilder.Entity<TrainingType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("training_types_pkey");

            entity.ToTable("training_types");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Basecost)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("basecost");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duration).HasColumnName("duration");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Biography)
                .HasColumnType("character varying")
                .HasColumnName("biography");
            entity.Property(e => e.Email)
                .HasColumnType("character varying")
                .HasColumnName("email");
            entity.Property(e => e.GoogleId)
                .HasColumnType("character varying")
                .HasColumnName("google_id");
            entity.Property(e => e.Login)
                .HasColumnType("character varying")
                .HasColumnName("login");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasColumnType("character varying")
                .HasColumnName("password_hash");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Surname)
                .HasColumnType("character varying")
                .HasColumnName("surname");
            entity.Property(e => e.UserPhoto)
                .HasColumnType("character varying")
                .HasColumnName("user_photo");
            entity.Property(e => e.Zasluga1)
                .HasColumnType("character varying")
                .HasColumnName("zasluga1");
            entity.Property(e => e.Zasluga2)
                .HasColumnType("character varying")
                .HasColumnName("zasluga2");
            entity.Property(e => e.Zasluga3)
                .HasColumnType("character varying")
                .HasColumnName("zasluga3");
            entity.Property(e => e.Zasluga4)
                .HasColumnType("character varying")
                .HasColumnName("zasluga4");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("users_roleid_fkey");
        });

        modelBuilder.Entity<UserHorse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("user_horses_pkey");

            entity.ToTable("user_horses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Horseid).HasColumnName("horseid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Horse).WithMany(p => p.UserHorses)
                .HasForeignKey(d => d.Horseid)
                .HasConstraintName("user_horses_horseid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.UserHorses)
                .HasForeignKey(d => d.Userid)
                .HasConstraintName("user_horses_userid_fkey");
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wallet_pkey");

            entity.ToTable("wallet");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Summ)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("summ");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("wallet_user_id_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
