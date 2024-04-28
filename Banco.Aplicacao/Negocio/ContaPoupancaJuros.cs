namespace Banco.Dominio.Negocio
{
    public class ContaPoupancaJuros
    {
        public decimal Percentual { get; private set; }



        public decimal Juro2024()
        {
            Percentual = 0.003M;
            return Percentual;
        }
    }
}
