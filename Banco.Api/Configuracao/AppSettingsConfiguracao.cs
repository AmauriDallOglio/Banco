using Banco.Aplicacao.DTO;
using Banco.Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Banco.Api.Configuracao
{
    public static class AppSettingsConfiguracao
    {
        public static void Carregar(this IServiceCollection services, IConfiguration configuration)
        {
            AppSettingsDto appSettingsDto = configuration.Get<AppSettingsDto>() ?? new AppSettingsDto();
            appSettingsDto = CarregaBancoDeDados(services, appSettingsDto);
            services.AddSingleton(appSettingsDto);
        }

        private static AppSettingsDto CarregaBancoDeDados(this IServiceCollection services, AppSettingsDto appSettingsDto)
        {
            var conexao = appSettingsDto.ConnectionStrings.ConexaoServidor;
            services.AddDbContext<BancoContexto>(opt => opt.UseSqlServer(conexao));
            return appSettingsDto;
        }

        private static void ImprimeAppSettingsDto(AppSettingsDto appSettings)
        {
            string json = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions { WriteIndented = true });
            // PrintaConsole.Alerta($"AppSettingsDto carregado: {json}");
        }
    }
}
