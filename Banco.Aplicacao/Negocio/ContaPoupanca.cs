using Banco.Dominio.Entidade;

namespace Banco.Dominio.Negocio
{
    public class ContaPoupanca : ContaBancaria
    {
        public decimal PercentualRendimento { get; private set; }

        public ContaPoupanca(Cliente cliente) : base(cliente)
        {

            ContaPoupancaJuros poupancaJuros = new ContaPoupancaJuros();
            PercentualRendimento = poupancaJuros.Juro2024();
        }


    }
}
