using Banco.Dominio.Entidade;

namespace Banco.Dominio.Negocio
{
    public class Deposito : Lancamento
    {
        private Deposito()
        {
        }

        public Deposito(decimal valor, DateTime data, ContaBancaria conta) : base(valor, data, conta)
        {
        }
    }
}
