using Banco.Dominio.Entidade;
using System.Text;

namespace Banco.Dominio.Negocio
{
    public class ContaCorrente : ContaBancaria
    {
        public decimal Limite { get; private set; }
        public decimal ValorTaxaManutencao { get; private set; }

        public ContaCorrente(Cliente cliente, decimal limite) : base(cliente)
        {
            ValorTaxaManutencao = 0.05M;
            Limite = limite;
        }

        public override void Sacar(decimal valor, string senha)
        {
            if (senha != Senha)
            {
                throw new Exception("Senha incorreta.");
            }

            var saque = new Saque(valor, DateTime.Now, this);

            var valorMaximoSaque = Saldo + Limite;

            if (valorMaximoSaque < saque.Valor)
            {
                throw new Exception("O saldo + limite não são suficientes para realizar o saque.");
            }

            Saldo -= saque.Valor;
            Lancamentos.Add(saque);
        }

        public override string VerExtrato()
        {
            var sb = new StringBuilder();

            sb.Append(base.VerExtrato());

            sb.AppendLine("Limite:        R$ " + Limite);
            sb.AppendLine("Saldo+Limite:  R$ " + (Limite + Saldo));

            return sb.ToString();
        }



    }
}
