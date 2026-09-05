using Banco.Dominio.Util;

namespace Banco.Dominio.Entidade
{
    public class Cliente
    {
        public int Id { get; set; }
        public string Nome { get; private set; }
        public string CPF { get; private set; }
        public string RG { get; private set; }
        public Endereco Endereco { get; private set; }
        public List<ContaBancaria> Contas { get; private set; }
        public DateTime DataCadastro { get; private set; }
        public DateTime DataAlteracao { get; private set; }
        public DateTime? Teste { get; private set; }
        protected Cliente()
        {
            Nome = string.Empty;
            CPF = string.Empty;
            RG = string.Empty;
            Endereco = null!;
            Contas = new List<ContaBancaria>();
            DataCadastro = DateTime.Now;
            DataAlteracao = DateTime.Now;
        }

        public Cliente(string nome, string cpf, string rg, Endereco endereco)
        {
            Nome = nome.ValidaStringVazia();
            CPF = cpf.ValidaStringVazia();
            RG = rg.ValidaStringVazia();
            Endereco = endereco ?? throw new Exception("Endereço deve ser informado.");
            Contas = new List<ContaBancaria>();
            DataCadastro = DateTime.Now;
            DataAlteracao = DateTime.Now;
        }

    }
}
