namespace Banco.Dominio.Util
{
    public class Enums
    {
        public enum SituacaoConta : int
        {
            Criada = 1,
            Aberta = 2,
            Encerrada = 3
        }

        public enum TipoConta
        {
            PessoaFisica = 1,
            PessoaJuridica = 2
        }
    }
}
