using Futebol.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Futebol.Api.Infrastructure;

public class FutebolDbContext : DbContext
{
    public FutebolDbContext(DbContextOptions<FutebolDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Organizador> Organizadores => Set<Organizador>();
    public DbSet<Jogador> Jogadores => Set<Jogador>();
    public DbSet<Sorteio> Sorteios => Set<Sorteio>();
    public DbSet<Time> Times => Set<Time>();
    public DbSet<TimeJogador> TimeJogadores => Set<TimeJogador>();
    public DbSet<Partida> Partidas => Set<Partida>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(FutebolDbContext).Assembly
        );

        modelBuilder.Entity<TimeJogador>()
            .HasIndex(x => new { x.TimeId, x.JogadorId })
            .IsUnique();

        // Garante que uma partida não tenha o mesmo time nos dois lados
        modelBuilder.Entity<Partida>()
            .HasCheckConstraint(
                "CK_Partida_Times_Diferentes",
                "TimeCasaId <> TimeVisitanteId"
            );

        base.OnModelCreating(modelBuilder);
    }
}
