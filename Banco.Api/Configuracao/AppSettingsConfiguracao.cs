using Banco.Aplicacao.DTO;
using System.Text.Json;

namespace Banco.Api.Configuracao
{
    public static class AppSettingsConfiguracao
    {
        public static void Carregar(this IServiceCollection services, IConfigurationRoot configuration)
        {


            AppSettingsDto appSettingsDto = configuration.Get<AppSettingsDto>() ?? new AppSettingsDto();

            //var caminhoSecret = @"C:\Amauri\GitHub\AutenticacaoJWT_Secret.txt";
            appSettingsDto = CarregaBancoDeDados(services, configuration, appSettingsDto);
            //  ImprimeAppSettingsDto(appSettingsDto);
            services.AddSingleton(appSettingsDto);
        }

        private static AppSettingsDto CarregaBancoDeDados(this IServiceCollection services, IConfigurationRoot configuration, AppSettingsDto appSettingsDto)
        {
            var conexaoCommand = appSettingsDto.ConnectionStrings.ConexaoServidor;
            var conexaoQuery = string.IsNullOrWhiteSpace(appSettingsDto.ConnectionStrings.ConexaoServidorQuery)
                ? conexaoCommand
                : appSettingsDto.ConnectionStrings.ConexaoServidorQuery;

            //PrintaConsole.Info($"Carregando conexão CommandContexto");
            services.AddDbContext<CommandContexto>(opt => opt.UseSqlServer(conexaoCommand));
            // PrintaConsole.Info($"Carregando conexão QueryContexto");
            services.AddDbContext<QueryContexto>(opt => opt.UseSqlServer(conexaoQuery));
            // PrintaConsole.Info($"Carregando conexão GenericoContexto");
            services.AddDbContext<GenericoContexto>(opt => opt.UseSqlServer(conexaoCommand));



            return appSettingsDto;
        }

        private static void ImprimeAppSettingsDto(AppSettingsDto appSettings)
        {
            string json = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions { WriteIndented = true });
            // PrintaConsole.Alerta($"AppSettingsDto carregado: {json}");
        }

    }
}
