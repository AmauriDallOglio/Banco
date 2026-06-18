namespace Banco.Aplicacao.DTO
{
    public class AppSettingsDto
    {
 
        public ConnectionStringsDto ConnectionStrings { get; set; } = new ConnectionStringsDto();
        public DatabaseDto Database { get; set; } = new DatabaseDto();
 
    } 

 
    public class ConnectionStringsDto
    {
        public string ConexaoServidor { get; set; } = string.Empty;
        public string ConexaoServidorQuery { get; set; } = string.Empty;
    }

    public class DatabaseDto
    {
        public bool AutoMigrate { get; set; }
        public bool Reinstall { get; set; }
    }
 
}
