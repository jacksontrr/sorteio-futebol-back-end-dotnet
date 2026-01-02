using Futebol.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Futebol.Api.Infrastructure.Mappings;

public class OrganizadorMap : IEntityTypeConfiguration<Organizador>
{
    public void Configure(EntityTypeBuilder<Organizador> builder)
    {
        builder.ToTable("Organizadores");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nome)
            .IsRequired()
            .HasMaxLength(150);

        builder.HasOne(o => o.User)
    .WithMany(u => u.Organizadores)
    .HasForeignKey(o => o.UserId)
    .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Jogadores)
            .WithOne(x => x.Organizador)
            .HasForeignKey(x => x.OrganizadorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
