using Futebol.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Futebol.Api.Infrastructure.Mappings;

public class JogadorMap : IEntityTypeConfiguration<Jogador>
{
    public void Configure(EntityTypeBuilder<Jogador> builder)
    {
        builder.ToTable("Jogadores");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Posicao)
            .HasMaxLength(50);

        builder.Property(x => x.Destaque)
            .HasDefaultValue(false);

        builder.Property(x => x.Peso)
            .HasDefaultValue(false);

        builder.Property(x => x.Ativo)
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}
