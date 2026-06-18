namespace Banco.Dominio.Entidade
{
    public abstract class Lancamento
    {
        public int Id { get; set; }
        public decimal Valor { get; init; }
        public DateTime Data { get; init; }
        public int ContaBancariaId { get; private set; }
        public ContaBancaria Conta { get; init; }

        protected Lancamento()
        {
            Conta = null!;
        }

        public Lancamento(decimal valor, DateTime data, ContaBancaria conta)
        {
            Data = data;
            Conta = conta ?? throw new ArgumentNullException(nameof(conta));
            Valor = valor > 0 ? valor : throw new Exception("Valor do lançamento deve ser maior que zero.");
        }
    }
}
