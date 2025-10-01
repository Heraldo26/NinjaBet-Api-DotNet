using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NinjaBet_Dmain.Entities;

namespace NinjaBet_Infrastructure.Persistence.Configurations
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            //builder.ToTable("Usuarios");

            //builder.HasKey(u => u.Id);

            //builder.Property(u => u.Username)
            //    .IsRequired()
            //    .HasMaxLength(50);

            //builder.Property(u => u.PasswordHash)
            //    .IsRequired()
            //    .HasMaxLength(255);

            //builder.Property(u => u.Perfil)
            //    .HasConversion<int>() // Salva enum como inteiro
            //    .IsRequired();

            //builder.Property(u => u.Ativo)
            //    .HasDefaultValue(true);

            //builder.Property(u => u.DataCriacao)
            //    .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
