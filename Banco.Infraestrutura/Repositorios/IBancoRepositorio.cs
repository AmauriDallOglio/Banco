using Banco.Dominio.Entidade;
 

namespace Banco.Infraestrutura.Repositorios
{
    public interface IBancoRepositorio
    {
        Task<Cliente> AdicionarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default);
        Task<List<Cliente>> ObterClientesAsync(CancellationToken cancellationToken = default);
    }
}
