using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using SistemaHEAVELYBackend.Models;


namespace SistemaHEAVELYBackend.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Alocaco> Alocacoes { get; set; }

    public virtual DbSet<DadosAcademico> DadosAcademicos { get; set; }

    public virtual DbSet<DadosAdministrativo> DadosAdministrativos { get; set; }

    public virtual DbSet<EventosProva> EventosProvas { get; set; }

    public virtual DbSet<Materia> Materias { get; set; }

    public virtual DbSet<Perfi> Perfis { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Alocaco>(entity =>
        {
            entity.HasKey(e => e.IdAlocacao).HasName("alocacoes_pkey");

            entity.Property(e => e.DataInscricao).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.IdEventoNavigation).WithMany(p => p.Alocacos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("alocacoes_id_evento_fkey");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Alocacos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("alocacoes_id_usuario_fkey");
        });

        modelBuilder.Entity<DadosAcademico>(entity =>
        {
            entity.HasKey(e => e.IdDadosAcademico).HasName("dados_academicos_pkey");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.DadosAcademico)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dados_academicos_id_usuario_fkey");
            entity.Property(e => e.CursoFormacao).HasColumnName("curso_formacao");    
        });

        modelBuilder.Entity<DadosAdministrativo>(entity =>
        {
            entity.HasKey(e => e.IdDadosAdministrativo).HasName("dados_administrativo_pkey");

            entity.Property(e => e.StatusAprovacao).HasDefaultValueSql("'pendente'::character varying");

            entity.HasOne(d => d.AprovadoPorNavigation).WithMany(p => p.DadosAdministrativoAprovadoPorNavigations).HasConstraintName("dados_administrativo_aprovado_por_fkey");

            entity.HasOne(d => d.IdUsuarioNavigation).WithOne(p => p.DadosAdministrativoIdUsuarioNavigation)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("dados_administrativo_id_usuario_fkey");
        });

        modelBuilder.Entity<EventosProva>(entity =>
        {
            entity.HasKey(e => e.IdEvento).HasName("eventos_prova_pkey");

            entity.Property(e => e.StatusEvento).HasDefaultValueSql("'ATIVO'::character varying");

            entity.HasOne(d => d.IdCriadorAdminNavigation).WithMany(p => p.EventosProvas)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("eventos_prova_id_criador_admin_fkey");
        });

        modelBuilder.Entity<Materia>(entity =>
        {
            entity.HasKey(e => e.IdMateria).HasName("materias_pkey");
        });

        modelBuilder.Entity<Perfi>(entity =>
        {
            entity.HasKey(e => e.IdPerfil).HasName("perfis_pkey");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("usuarios_pkey");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.StatusConta).HasDefaultValueSql("'pendente'::character varying");

            entity.HasOne(d => d.IdPerfilNavigation).WithMany(p => p.Usuarios)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usuarios_id_perfil_fkey");

            entity.HasMany(d => d.IdMateria).WithMany(p => p.IdUsuarios)
                .UsingEntity<Dictionary<string, object>>(
                    "UsuarioMateria",
                    r => r.HasOne<Materia>().WithMany()
                        .HasForeignKey("IdMateria")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("usuario_materias_id_materia_fkey"),
                    l => l.HasOne<Usuario>().WithMany()
                        .HasForeignKey("IdUsuario")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("usuario_materias_id_usuario_fkey"),
                    j =>
                    {
                        j.HasKey("IdUsuario", "IdMateria").HasName("usuario_materias_pkey");
                        j.ToTable("usuario_materias");
                        j.IndexerProperty<int>("IdUsuario").HasColumnName("id_usuario");
                        j.IndexerProperty<int>("IdMateria").HasColumnName("id_materia");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}