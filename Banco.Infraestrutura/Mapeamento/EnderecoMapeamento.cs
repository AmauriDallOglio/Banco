using Banco.Dominio.Entidade;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Banco.Infraestrutura.Mapeamento
{
    public static class EnderecoMapeamento
    {
        public static void Configure(OwnedNavigationBuilder<Cliente, Endereco> builder)
        {
            builder.WithOwner();
            builder.ToTable("Endereco");
            builder.Property(e => e.Id).HasColumnName("Id");
            builder.Property(e => e.Logradouro).HasColumnName("Logradouro").IsRequired().HasMaxLength(200);
            builder.Property(e => e.CEP).HasColumnName("CEP").IsRequired().HasMaxLength(20);
            builder.Property(e => e.Cidade).HasColumnName("Cidade").IsRequired().HasMaxLength(100);
            builder.Property(e => e.Estado).HasColumnName("Estado").IsRequired().HasMaxLength(100);
        }
    }
}
