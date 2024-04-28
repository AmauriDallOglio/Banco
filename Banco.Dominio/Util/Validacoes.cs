namespace Banco.Dominio.Util
{
    public static class Validacoes
    {
        public static string ValidaStringVazia(this string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ?
                             throw new Exception("Propriedade deve estar preenchida.")
                             : texto;
        }
    }
}
