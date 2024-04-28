using Banco.Dominio.Entidade;

namespace Banco.Dominio.Negocio
{
    public class Saque : Lancamento
    {
        public Saque(decimal valor, DateTime data, ContaBancaria conta) : base(valor, data, conta)
        {

        }
    }
}
