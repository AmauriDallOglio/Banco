namespace Banco.Aplicacao.DTO
{
    public class AppSettingsDto
    {
 
        public ConnectionStringsDto ConnectionStrings { get; set; } = new ConnectionStringsDto();
 
    } 

 
    public class ConnectionStringsDto
    {
        public string ConexaoServidor { get; set; } = string.Empty;
        public string ConexaoServidorQuery { get; set; } = string.Empty;
    }

 
}
