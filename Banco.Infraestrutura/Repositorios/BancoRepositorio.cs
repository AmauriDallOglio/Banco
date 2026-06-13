using Banco.Dominio.Entidade;
using Banco.Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Banco.Infraestrutura.Repositorios
{
    public class BancoRepositorio : IBancoRepositorio
    {
        private readonly BancoContexto _contexto;

        public BancoRepositorio(BancoContexto contexto)
        {
            _contexto = contexto;
        }

        public async Task<Cliente> AdicionarClienteAsync(Cliente cliente, CancellationToken cancellationToken = default)
        {
            _contexto.Clientes.Add(cliente);
            await _contexto.SaveChangesAsync(cancellationToken);
            return cliente;
        }

        public Task<List<Cliente>> ObterClientesAsync(CancellationToken cancellationToken = default)
        {
            return _contexto.Clientes
                .Include(c => c.Contas)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
