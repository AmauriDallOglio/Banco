using Banco.Aplicacao.DTO;
using Banco.Dominio.Entidade;
using Banco.Dominio.Negocio;
using Banco.Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Banco.Api.Configuracao
{
    public static class Migracao
    {
        private static readonly string[] OperacoesDestrutivas =
        {
            "DROP TABLE",
            "DROP COLUMN",
            "ALTER COLUMN",
            "TRUNCATE TABLE"
        };

        public static async Task ExecutarMigracaoAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var contexto = scope.ServiceProvider.GetRequiredService<BancoContexto>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Database.Migracao");
            var configuracao = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var appSettings = scope.ServiceProvider.GetRequiredService<AppSettingsDto>();

            var databaseOptions = CarregarDatabaseOptions(configuracao, appSettings);

            if (!databaseOptions.AutoMigrate)
            {
                logger.LogInformation("Migração automática desativada por configuração Database:AutoMigrate=false.");
                return;
            }

            try
            {
                logger.LogInformation("Iniciando validação e migração do banco de dados.");

                if (databaseOptions.Reinstall)
                {
                    logger.LogWarning("Database:Reinstall=true. O banco será removido e recriado.");
                    await contexto.Database.EnsureDeletedAsync();
                }

                var pendentes = (await contexto.Database.GetPendingMigrationsAsync()).ToList();
                var aplicadas = (await contexto.Database.GetAppliedMigrationsAsync()).ToList();

                if (aplicadas.Count == 0 && pendentes.Count > 0 && await ExisteTabelaAsync(contexto, "Clientes"))
                {
                    logger.LogWarning("Banco existente sem histórico de migrations detectado. Registrando baseline da migration {Migration}.", pendentes[0]);
                    await RegistrarBaselineAsync(contexto, pendentes[0]);
                    pendentes = (await contexto.Database.GetPendingMigrationsAsync()).ToList();
                }

                if (pendentes.Count == 0)
                {
                    logger.LogInformation("Nenhuma migração pendente encontrada.");
                }
                else if (await ExisteAlteracaoDestrutivaAsync(contexto, logger))
                {
                    var caminhoScript = await GerarScriptMigracaoAsync(contexto, app.Environment.ContentRootPath);
                    logger.LogError("Migração destrutiva detectada. Script gerado em {CaminhoScript}. Revise e execute manualmente.", caminhoScript);
                    return;
                }
                else
                {
                    logger.LogInformation("Aplicando {Quantidade} migração(ões): {Migracoes}", pendentes.Count, string.Join(", ", pendentes));
                    await contexto.Database.MigrateAsync();
                    logger.LogInformation("Migrações aplicadas com sucesso.");
                }

                await ExecutarSeedAsync(contexto, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao inicializar banco de dados.");
                throw;
            }
        }

        private static DatabaseDto CarregarDatabaseOptions(IConfiguration configuration, AppSettingsDto appSettings)
        {
            var options = appSettings.Database;
            configuration.GetSection("databse").Bind(options);
            configuration.GetSection("Database").Bind(options);
            return options;
        }

        private static async Task<bool> ExisteAlteracaoDestrutivaAsync(BancoContexto contexto, ILogger logger)
        {
            var script = await GerarScriptMigracaoAsync(contexto, null);

            foreach (var operacao in OperacoesDestrutivas)
            {
                if (script.Contains(operacao, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("Operação destrutiva encontrada no script de migração: {Operacao}", operacao);
                    return true;
                }
            }

            return false;
        }

        private static async Task<string> GerarScriptMigracaoAsync(BancoContexto contexto, string? contentRootPath)
        {
            var aplicadas = (await contexto.Database.GetAppliedMigrationsAsync()).ToList();
            var ultimaAplicada = aplicadas.LastOrDefault();
            var migrator = contexto.GetService<IMigrator>();
            var script = migrator.GenerateScript(ultimaAplicada);

            if (contentRootPath is null)
            {
                return script;
            }

            var pasta = Path.Combine(contentRootPath, "DatabaseScripts");
            Directory.CreateDirectory(pasta);

            var caminho = Path.Combine(pasta, $"migracao-destrutiva-{DateTime.Now:yyyyMMddHHmmss}.sql");
            await File.WriteAllTextAsync(caminho, script);

            return caminho;
        }

        private static async Task<bool> ExisteTabelaAsync(BancoContexto contexto, string tabela)
        {
            var conexao = contexto.Database.GetDbConnection();
            var deveFechar = conexao.State == System.Data.ConnectionState.Closed;

            if (deveFechar)
            {
                await conexao.OpenAsync();
            }

            try
            {
                await using var comando = conexao.CreateCommand();
                comando.CommandText = "SELECT CASE WHEN OBJECT_ID(@tabela, 'U') IS NULL THEN 0 ELSE 1 END";

                var parametro = comando.CreateParameter();
                parametro.ParameterName = "@tabela";
                parametro.Value = tabela;
                comando.Parameters.Add(parametro);

                var resultado = await comando.ExecuteScalarAsync();
                return Convert.ToInt32(resultado) == 1;
            }
            finally
            {
                if (deveFechar)
                {
                    await conexao.CloseAsync();
                }
            }
        }

        private static Task RegistrarBaselineAsync(BancoContexto contexto, string migrationId)
        {
            return contexto.Database.ExecuteSqlRawAsync(
                """
                IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
                BEGIN
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,
                        [ProductVersion] nvarchar(32) NOT NULL,
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                END;

                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0})
                BEGIN
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ({0}, N'8.0.0');
                END;
                """,
                migrationId);
        }

        private static async Task ExecutarSeedAsync(BancoContexto contexto, ILogger logger)
        {
            if (await contexto.Clientes.AnyAsync())
            {
                logger.LogInformation("Seed ignorado: tabela Clientes já possui dados.");
                return;
            }

            logger.LogInformation("Executando seed inicial.");

            var endereco = new Endereco("Rua Inicial", "00000-000", "Sao Paulo", "SP");
            var cliente = new Cliente("Cliente Inicial", "00000000000", "000000000", endereco);
            var conta = new ContaCorrente(cliente, 500);
            conta.Abrir("senha123");

            cliente.Contas.Add(conta);
            contexto.Clientes.Add(cliente);

            await contexto.SaveChangesAsync();

            logger.LogInformation("Seed inicial concluído.");
        }
    }
}
