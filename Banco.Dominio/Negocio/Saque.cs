using Banco.Dominio.Entidade;

namespace Banco.Dominio.Negocio
{
    public class Saque : Lancamento
    {
        private Saque()
        {
        }

        public Saque(decimal valor, DateTime data, ContaBancaria conta) : base(valor, data, conta)
        {
        }
    }
}
