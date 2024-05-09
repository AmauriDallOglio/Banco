using Banco.Dominio.Negocio;
using Banco.Dominio.Util;
using System.Text;
using System.Text.RegularExpressions;
using static Banco.Dominio.Util.Enums;

namespace Banco.Dominio.Entidade
{
    public abstract class ContaBancaria
    {

        public int NumeroConta { get; init; }
        public int DigitoVerificador { get; init; }
        public decimal Saldo { get; protected set; }
        public DateTime? DataAbertura { get; private set; }
        public DateTime? DataEncerramento { get; private set; }
        public Enums.SituacaoConta Situacao { get; private set; }
        public string Senha { get; private set; }
        public decimal Limite { get; private set; }
        public Cliente Cliente { get; init; }

        private TipoConta TipoConta { get; set; }
 
        private double Credito { get; set; }
        private string Nome { get; set; }

        public List<Lancamento> Lancamentos { get; private set; }

        public ContaBancaria(Cliente cliente)
        {
            Random random = new Random();
            NumeroConta = random.Next(50000, 100000);
            DigitoVerificador = random.Next(0, 9);

            Situacao = Enums.SituacaoConta.Criada;

            Cliente = cliente ?? throw new Exception("Cliente deve ser informado.");
        }

        public void Abrir(string senha)
        {
            SetaSenha(senha);

            Situacao = Enums.SituacaoConta.Aberta;
            DataAbertura = DateTime.Now;
            Lancamentos = new List<Lancamento>();
        }

        private void SetaSenha(string senha)
        {
            senha = senha.ValidaStringVazia();

            if (!Regex.IsMatch(senha, @"^(?=.*?[a-z])(?=.*?[0-9]).{8,}$"))
            {
                throw new Exception("Senha inválida");
            }

            Senha = senha;
        }

        public void Depositar(decimal valor)
        {
            var deposito = new Deposito(valor, DateTime.Now, this);

            Saldo += deposito.Valor;
            Lancamentos.Add(deposito);
        }

        public virtual void Sacar(decimal valor, string senha)
        {
            if (senha != Senha)
            {
                throw new Exception("Senha incorreta.");
            }

            var saque = new Saque(valor, DateTime.Now, this);

            if (Saldo < saque.Valor)
            {
                throw new Exception("Saldo indisponível.");
            }

            Saldo -= saque.Valor;
            Lancamentos.Add(saque);
        }

        public string VerSaldo()
        {

            return $"Saldo atual:   R$ {Saldo}";
        }

        public virtual string VerExtrato()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"-- Extrato - Lançamentos --");

            foreach (var lancamento in Lancamentos)
            {

                // Adiciona o tipo de lançamento
                sb.Append(lancamento.GetType().Name.PadRight(14) + " Data: ");

                // Adiciona a data formatada
                sb.Append(lancamento.Data.ToString("dd/MM/yyyy hh:mm:ss") + " / ");

                // Adiciona o sinal (-) para saques e (+) para depósitos
                if (lancamento is Saque)
                    sb.Append(" (-) ".PadRight(1));
                else if (lancamento is Deposito)
                    sb.Append(" (+) ".PadRight(1));
                else
                    sb.Append(" ".PadRight(1)); // Adiciona espaços em branco se não for um Saque ou Depósito

                // Adiciona o valor do lançamento
                sb.AppendLine("R$ " + lancamento.Valor.ToString("0.00"));


            }

            sb.AppendLine("Saldo final:   R$ " + Saldo);

            return sb.ToString();
        }




    }
}
