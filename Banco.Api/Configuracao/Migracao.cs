using Banco.Aplicacao.DTO;
using Banco.Dominio.Entidade;
using Banco.Dominio.Negocio;
using Banco.Dominio.Util;
using Banco.Infraestrutura.Contexto;
using Microsoft.EntityFrameworkCore;

namespace Banco.Api.Configuracao
{

    public static class Migracao
    {

        private static readonly string[] OperacoesDestrutivas =
        {
            "DROP TABLE",      // Remove tabela inteira
            "DROP COLUMN",     // Remove coluna inteira
            "ALTER COLUMN",    // Altera estrutura da coluna
            "TRUNCATE TABLE"   // Deleta todos os dados da tabela
        };


        public static async Task ExecutarMigracaoAsync(this WebApplication app)
        {
            // Cria um escopo de injeção de dependência isolado para evitar vazamento de memória
            // Cada serviço obtido aqui será descartado ao final do método
            using var scope = app.Services.CreateScope();

            // Obtém os serviços registrados na DI (Dependency Injection).
            // O BancoContexto é o DbContext que gerencia a comunicação com o banco de dados.
            // Ele conhece todas as entidades através do mapeamento definido em OnModelCreating()
            BancoContexto contexto = scope.ServiceProvider.GetRequiredService<BancoContexto>();

            // Logger para registrar informações, avisos e erros do processo
            ILogger logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Database.Migracao");

            // Configuração da aplicação (appsettings.json, appsettings.Development.json, etc)
            IConfiguration configuracao = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            
            // DTO com configurações mapeadas e validadas
            AppSettingsDto appSettings = scope.ServiceProvider.GetRequiredService<AppSettingsDto>();

            // Carrega as opções do banco de dados (AutoMigrate, DeletaDatabase, etc)
            DatabaseDto databaseOptions = CarregarDatabaseOptions(configuracao, appSettings);

            // Valida se a migração automática está ativada
            if (!databaseOptions.AutoMigrate)
            {
                logger.LogInformation("Migração automática desativada por configuração Database:AutoMigrate=false.");
                return;
            }

            try
            {
                logger.LogInformation("Iniciando validação e migração do banco de dados.");

                // DeletaDatabase=true remove o banco e o recria usando todas as migrations.
                // Reinstall continua aceito como compatibilidade com a configuração antiga.
                if (databaseOptions.DeletaDatabase || databaseOptions.Reinstall)
                {
                    logger.LogWarning("Database:DeletaDatabase=true. O banco será removido e recriado.");
                    await contexto.Database.EnsureDeletedAsync();
                    logger.LogInformation("Banco deletado. Criando banco e tabelas conforme os mapeamentos.");
                    await contexto.Database.MigrateAsync();
                    logger.LogInformation("Banco e tabelas criados com sucesso.");
                }
                else
                {
                    // MigrateAsync cria o banco se necessário e aplica as migrations pendentes.
                    // As migrations controlam inclusão, alteração e remoção de colunas/tabelas.
                    if (!await contexto.Database.CanConnectAsync())
                    {
                        logger.LogInformation("Banco não encontrado. Criando banco, tabelas e histórico de migrations.");
                        await contexto.Database.MigrateAsync();
                        logger.LogInformation("Banco criado e atualizado com sucesso.");
                    }
                    else
                    {
                        var pendentes = (await contexto.Database.GetPendingMigrationsAsync()).ToList();
                        var aplicadas = (await contexto.Database.GetAppliedMigrationsAsync()).ToList();

                        // Banco criado manualmente antes do histórico do EF Core.
                        if (aplicadas.Count == 0 && pendentes.Count > 0 && await ExisteTabelaAsync(contexto, "Cliente"))
                        {
                            logger.LogWarning("Banco existente sem histórico de migrations detectado. Registrando baseline da migration {Migration}.", pendentes[0]);
                            await RegistrarBaselineAsync(contexto, pendentes[0]);
                            pendentes = (await contexto.Database.GetPendingMigrationsAsync()).ToList();
                        }

                        if (pendentes.Count == 0)
                        {
                            logger.LogInformation("Nenhuma atualização de tabela pendente encontrada.");
                        }
                        else
                        {
                            logger.LogInformation("Atualizando tabelas com {Quantidade} migration(ões): {Migracoes}", pendentes.Count, string.Join(", ", pendentes));
                            await contexto.Database.MigrateAsync();
                            logger.LogInformation("Atualização das tabelas concluída.");
                        }
                    }
                }

                // Após aplicar migrações, executa o seed (dados iniciais)
                // Isto garante que o banco tem dados necessários para funcionar
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
            // Obtém a instância de DatabaseDto já criada (pode ter valores padrão)
            var options = appSettings.Database;
            
            // Faz binding da seção "databse" (note o typo) - isto permite compatibilidade com configuração antiga
            configuration.GetSection("databse").Bind(options);
            
            // Faz binding da seção "Database" (correta) - isto sobrescreve valores anteriores
            // O Bind() preenche as propriedades da classe options com os valores da configuração
            configuration.GetSection("Database").Bind(options);
            
            return options;
        }


        //private static async Task<bool> ExisteAlteracaoDestrutivaAsync(BancoContexto contexto, ILogger logger)
        //{
        //    // Gera o script SQL da migração sem salvar em arquivo
        //    // contentRootPath = null faz com que o método retorne apenas o script em memória
        //    var script = await ExecutaClasseScriptMigracao(contexto, null);

        //    // Procura por cada palavra-chave de operação destrutiva
        //    foreach (var operacao in OperacoesDestrutivas)
        //    {
        //        // StringComparison.OrdinalIgnoreCase = case-insensitive, mas baseado na ordenação ordinal (ASCII)
        //        if (script.Contains(operacao, StringComparison.OrdinalIgnoreCase))
        //        {
        //            logger.LogWarning("Operação destrutiva encontrada no script de migração: {Operacao}", operacao);
        //            return true; // Encontrou operação perigosa
        //        }
        //    }

        //    return false; // Script é seguro
        //}


        //private static async Task<string> ExecutaClasseScriptMigracao(BancoContexto contexto, string? contentRootPath)
        //{
        //    // Obtém lista de todas as migrações já aplicadas ao banco
        //    // Consultando a tabela __EFMigrationsHistory
        //    var aplicadas = (await contexto.Database.GetAppliedMigrationsAsync()).ToList();
            
        //    // Pega a última migração aplicada (ou null se nenhuma foi aplicada)
        //    // GenerateScript() usará isto como ponto de partida
        //    var ultimaAplicada = aplicadas.LastOrDefault();
            
        //    // Obtém o serviço IMigrator do contexto
        //    // Este serviço sabe como gerar scripts SQL baseado no modelo EF Core
        //    var migrator = contexto.GetService<IMigrator>();
            
        //    // Gera o script SQL de TODAS as migrações pendentes a partir da última aplicada
        //    // Por exemplo:
        //    // - Se nenhuma foi aplicada (ultimaAplicada=null), gera script de TODAS as migrações
        //    // - Se "Migration001" foi aplicada, gera script de "Migration002", "Migration003", etc
        //    // O script contém todos os comandos ALTER TABLE, CREATE TABLE, etc necessários
        //    var script = migrator.GenerateScript(ultimaAplicada);

        //    // Se contentRootPath for nulo, apenas retorna o script em memória
        //    // Isto é usado quando queremos verificar o script SEM salvá-lo em arquivo
        //    if (contentRootPath is null)
        //    {
        //        return script;
        //    }

        //    // Cria pasta "DatabaseScripts" no raiz da aplicação se não existir
        //    // Isto será usado para armazenar scripts de migrações destrutivas
        //    var pasta = Path.Combine(contentRootPath, "DatabaseScripts");
        //    Directory.CreateDirectory(pasta);

        //    // Cria nome do arquivo com timestamp para evitar colisões
        //    // Formato: migracao-destrutiva-20260604143022.sql
        //    var caminho = Path.Combine(pasta, $"migracao-destrutiva-{DateTime.Now:yyyyMMddHHmmss}.sql");
            
        //    // Salva o script em arquivo para que o DBA/desenvolvedor possa revisar antes de executar
        //    await File.WriteAllTextAsync(caminho, script);

        //    // Retorna o caminho do arquivo salvo
        //    return caminho;
        //}

        private static async Task<bool> ExisteTabelaAsync(BancoContexto contexto, string tabela)
        {
            // Obtém a conexão SQL diretamente do contexto
            // É uma instância de SqlConnection (ou a implementação do provedor configurado)
            var conexao = contexto.Database.GetDbConnection();
            
            // Verifica se a conexão está fechada
            var deveFechar = conexao.State == System.Data.ConnectionState.Closed;

            // Se estava fechada, abre a conexão
            if (deveFechar)
            {
                await conexao.OpenAsync();
            }

            try
            {
                // Cria um comando SQL para executar na conexão
                await using var comando = conexao.CreateCommand();
                
                // Query SQL que verifica se a tabela existe
                // OBJECT_ID(@tabela, 'U') retorna o ID do objeto se for uma tabela (U = user table)
                // Se não existir, retorna NULL
                // CASE WHEN ... THEN 1 ELSE 0 END converte para 0 ou 1 para facilitar
                comando.CommandText = "SELECT CASE WHEN OBJECT_ID(@tabela, 'U') IS NULL THEN 0 ELSE 1 END";

                // Cria parâmetro para evitar SQL injection
                var parametro = comando.CreateParameter();
                parametro.ParameterName = "@tabela";
                parametro.Value = tabela;
                comando.Parameters.Add(parametro);

                // Executa a query e obtém o resultado (0 ou 1)
                var resultado = await comando.ExecuteScalarAsync();
                
                // Retorna true se resultado = 1 (tabela existe), false caso contrário
                return Convert.ToInt32(resultado) == 1;
            }
            finally
            {
                // Se foi necessário abrir a conexão, fecha-a agora
                // Isto garante que não fiquem conexões abertas
                if (deveFechar)
                {
                    await conexao.CloseAsync();
                }
            }
        }


        private static Task RegistrarBaselineAsync(BancoContexto contexto, string migrationId)
        {
            // ExecuteSqlRawAsync executa SQL bruto (direto) no banco de dados
            // Isto é necessário porque a operação precisa de DDL (Data Definition Language)
            // que o EF Core não pode fazer através de SaveChanges()
            return contexto.Database.ExecuteSqlRawAsync(
                """
                -- Verifica se a tabela de histórico de migrações existe
                IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
                BEGIN
                    -- Se não existir, cria a tabela
                    CREATE TABLE [__EFMigrationsHistory] (
                        [MigrationId] nvarchar(150) NOT NULL,        -- ID da migração (ex: 20260617234901_Inicial)
                        [ProductVersion] nvarchar(32) NOT NULL,      -- Versão do EF Core
                        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                    );
                END;

                -- Verifica se a migração já está registrada
                IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = {0})
                BEGIN
                    -- Se não estiver, insere o registro
                    -- {0} será substituído pelo migrationId passado como parâmetro
                    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                    VALUES ({0}, N'8.0.0');  -- Marca como aplicada com versão 8.0.0
                END;
                """,
                migrationId);
        }


        private static async Task ExecutarSeedAsync(BancoContexto contexto, ILogger logger)
        {
            // Verifica se a tabela Clientes já possui dados
            // AnyAsync() retorna true se houver pelo menos 1 cliente
            // Isto garante que o seed é idempotente (executar 2x = executar 1x)
            if (await contexto.Cliente.AnyAsync())
            {
                logger.LogInformation("Seed ignorado: tabela Clientes já possui dados.");
                return;
            }

            logger.LogInformation("Executando seed com dados mocados para todas as entidades.");

            var clientes = new List<Cliente>();
            var contas = new List<Conta>
            {
                new Conta(Enums.TipoConta.PessoaFisica, 2500.00, 1000.00, "Conta PF - João Silva"),
                new Conta(Enums.TipoConta.PessoaFisica, 7800.50, 2000.00, "Conta PF - Maria Santos"),
                new Conta(Enums.TipoConta.PessoaJuridica, 25000.00, 10000.00, "Conta PJ - Pedro Oliveira")
            };

            // ============================================================================
            // CLIENTE 1: João Silva
            // ============================================================================
            var endereco1 = new Endereco("Avenida Paulista, 1000", "01311-100", "São Paulo", "SP");
            var cliente1 = new Cliente("João Silva", "12345678900", "123456789", endereco1);

            // Conta Corrente para Cliente 1
            var contaCorrente1 = new ContaCorrente(cliente1, 5000M); // Limite de R$ 5.000
            contaCorrente1.Abrir("Senha@123");
            contaCorrente1.Depositar(10000M);      // Deposita R$ 10.000
            contaCorrente1.Sacar(2500M, "Senha@123"); // Saca R$ 2.500
            contaCorrente1.Depositar(1500M);      // Deposita R$ 1.500
            cliente1.Contas.Add(contaCorrente1);

            // Conta Poupança para Cliente 1
            var contaPoupanca1 = new ContaPoupanca(cliente1);
            contaPoupanca1.Abrir("Poupanca@456");
            contaPoupanca1.Depositar(50000M);     // Deposita R$ 50.000
            cliente1.Contas.Add(contaPoupanca1);

            clientes.Add(cliente1);

            // ============================================================================
            // CLIENTE 2: Maria Santos
            // ============================================================================
            var endereco2 = new Endereco("Rua Oscar Freire, 500", "01426-000", "São Paulo", "SP");
            var cliente2 = new Cliente("Maria Santos", "98765432100", "987654321", endereco2);

            // Conta Corrente para Cliente 2
            var contaCorrente2 = new ContaCorrente(cliente2, 3000M); // Limite de R$ 3.000
            contaCorrente2.Abrir("Maria@2024");
            contaCorrente2.Depositar(25000M);     // Deposita R$ 25.000
            contaCorrente2.Sacar(5000M, "Maria@2024"); // Saca R$ 5.000
            contaCorrente2.Depositar(3000M);     // Deposita R$ 3.000
            contaCorrente2.Sacar(1000M, "Maria@2024"); // Saca R$ 1.000
            cliente2.Contas.Add(contaCorrente2);

            // Conta Poupança para Cliente 2
            var contaPoupanca2 = new ContaPoupanca(cliente2);
            contaPoupanca2.Abrir("Poup@2024");
            contaPoupanca2.Depositar(75000M);    // Deposita R$ 75.000
            contaPoupanca2.Depositar(10000M);   // Deposita R$ 10.000
            cliente2.Contas.Add(contaPoupanca2);

            clientes.Add(cliente2);

            // ============================================================================
            // CLIENTE 3: Pedro Oliveira
            // ============================================================================
            var endereco3 = new Endereco("Rua Augusta, 2500", "01305-100", "São Paulo", "SP");
            var cliente3 = new Cliente("Pedro Oliveira", "55544433300", "555444333", endereco3);

            // Conta Corrente para Cliente 3
            var contaCorrente3 = new ContaCorrente(cliente3, 2000M); // Limite de R$ 2.000
            contaCorrente3.Abrir("Pedro#2024");
            contaCorrente3.Depositar(15000M);    // Deposita R$ 15.000
            contaCorrente3.Sacar(3000M, "Pedro#2024"); // Saca R$ 3.000
            contaCorrente3.Depositar(2000M);    // Deposita R$ 2.000
            cliente3.Contas.Add(contaCorrente3);

            // Conta Poupança para Cliente 3
            var contaPoupanca3 = new ContaPoupanca(cliente3);
            contaPoupanca3.Abrir("PedroPoup2024");
            contaPoupanca3.Depositar(100000M);  // Deposita R$ 100.000
            contaPoupanca3.Sacar(20000M, "PedroPoup2024"); // Saca R$ 20.000
            cliente3.Contas.Add(contaPoupanca3);

            clientes.Add(cliente3);

            // ============================================================================
            // INSERINDO TODOS OS CLIENTES E SUAS CONTAS NO CONTEXTO
            // ============================================================================
            // Adiciona todos os clientes ao contexto
            // O EF Core rastreará automaticamente:
            // 1. Os clientes (tabela Clientes)
            // 2. As contas (tabela Contas)
            // 3. Os lançamentos (tabela Lancamentos) - Depósitos e Saques criados pelos métodos Depositar() e Sacar()
            foreach (var cliente in clientes)
            {
                contexto.Cliente.Add(cliente);
            }

            // A entidade Conta é independente das contas bancárias vinculadas a Cliente.
            // Por isso, ela precisa ser adicionada explicitamente ao seu DbSet.
            // Endereco é inserido automaticamente pelo EF Core como entidade owned de Cliente.
            contexto.Conta.AddRange(contas);

            // SaveChangesAsync() executa todos os INSERTs em uma única transação
            // Ordem de inserção respeitada pelo EF Core:
            // 1. Clientes
            // 2. Contas (com FK para Clientes)
            // 3. Lançamentos (com FK para Contas)
            // O mapeamento (ClienteMapeamento, ContaBancariaMapeamento, etc) garante que
            // os dados sejam inseridos nas colunas corretas
            await contexto.SaveChangesAsync();

            logger.LogInformation("Seed concluído com sucesso. Clientes: {Clientes}, contas: {Contas}, contas bancárias: {ContasBancarias}, lançamentos: {Lancamentos}",
                clientes.Count,
                contas.Count,
                clientes.SelectMany(cliente => cliente.Contas).Count(),
                clientes.SelectMany(cliente => cliente.Contas).SelectMany(conta => conta.Lancamentos).Count());
            logger.LogInformation("Clientes: {Clientes}", string.Join(", ", clientes.Select(c => c.Nome)));
        }
    }
}


/*
 * 
 
use banco

select * from __EFMigrationsHistory

Select * from Cliente

select * from Conta

select * from Lancamento

select * from Endereco

select * from ContaBancaria


--Criação da migração

1. Crie uma nova pasta chamada: Migrations
Exemplo: Banco.Infraestrutura/Migrations/

2. Executar
dotnet ef migrations add InicialModeloAtual --project Banco.Infraestrutura --startup-project Banco.Api --output-dir Migrations
Exemplo: O EF Core criará automaticamente dentro de Banco.Infraestrutura/Migrations:
- InicialModeloAtual.cs
- BancoContextoModelSnapshot.cs

3. Depois, para aplicar ao banco:
dotnet ef database update --project Banco.Infraestrutura --startup-project Banco.Api



PS C:\Amauri\GitHub\Banco> dotnet ef migrations add InicialModeloAtual --project Banco.Infraestrutura --startup-project Banco.Api --output-dir Migrations
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'
PS C:\Amauri\GitHub\Banco> dotnet ef database update --project Banco.Infraestrutura --startup-project Banco.Api
Build started...
Build succeeded.
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (9ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (9ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT OBJECT_ID(N'[__EFMigrationsHistory]');
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT 1
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (0ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT OBJECT_ID(N'[__EFMigrationsHistory]');
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (9ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT [MigrationId], [ProductVersion]
      FROM [__EFMigrationsHistory]
      ORDER BY [MigrationId];
info: Microsoft.EntityFrameworkCore.Migrations[20402]
      Applying migration '20260905015834_InicialModeloAtual'.
Applying migration '20260905015834_InicialModeloAtual'.
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (8ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE TABLE [Cliente] (
          [Id] int NOT NULL IDENTITY,
          [Nome] nvarchar(200) NOT NULL,
          [CPF] nvarchar(20) NOT NULL,
          [RG] nvarchar(20) NOT NULL,
          [DataCadastro] datetime2 NOT NULL,
          [DataAlteracao] datetime2 NOT NULL,
          [Teste] datetime2 NULL,
          CONSTRAINT [PK_Cliente] PRIMARY KEY ([Id])
      );
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE TABLE [Conta] (
          [Id] int NOT NULL IDENTITY,
          [TipoConta] int NOT NULL,
          [Saldo] float(18) NOT NULL,
          [Credito] float(18) NOT NULL,
          [Nome] nvarchar(200) NOT NULL,
          [DataCadastro] datetime2 NOT NULL,
          [DataAlteracao] datetime2 NOT NULL,
          CONSTRAINT [PK_Conta] PRIMARY KEY ([Id])
      );
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (3ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE TABLE [ContaBancaria] (
          [Id] int NOT NULL IDENTITY,
          [NumeroConta] int NOT NULL,
          [DigitoVerificador] int NOT NULL,
          [Saldo] decimal(18,2) NOT NULL,
          [DataAbertura] datetime2 NULL,
          [DataEncerramento] datetime2 NULL,
          [Situacao] int NOT NULL,
          [Senha] nvarchar(200) NOT NULL,
          [Limite] decimal(18,2) NOT NULL,
          [ClienteId] int NOT NULL,
          [DataCadastro] datetime2 NOT NULL,
          [DataAlteracao] datetime2 NOT NULL,
          [Tipo] nvarchar(13) NOT NULL,
          [ValorTaxaManutencao] decimal(18,2) NULL,
          [PercentualRendimento] decimal(18,6) NULL,
          CONSTRAINT [PK_ContaBancaria] PRIMARY KEY ([Id]),
          CONSTRAINT [FK_ContaBancaria_Cliente_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Cliente] ([Id]) ON DELETE CASCADE
      );
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE TABLE [Endereco] (
          [ClienteId] int NOT NULL,
          [Id] int NOT NULL,
          [Logradouro] nvarchar(200) NOT NULL,
          [CEP] nvarchar(20) NOT NULL,
          [Cidade] nvarchar(100) NOT NULL,
          [Estado] nvarchar(100) NOT NULL,
          [DataCadastro] datetime2 NOT NULL,
          [DataAlteracao] datetime2 NOT NULL,
          CONSTRAINT [PK_Endereco] PRIMARY KEY ([ClienteId]),
          CONSTRAINT [FK_Endereco_Cliente_ClienteId] FOREIGN KEY ([ClienteId]) REFERENCES [Cliente] ([Id]) ON DELETE CASCADE
      );
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (2ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE TABLE [Lancamento] (
          [Id] int NOT NULL IDENTITY,
          [Valor] decimal(18,2) NOT NULL,
          [Data] datetime2 NOT NULL,
          [ContaBancariaId] int NOT NULL,
          [DataCadastro] datetime2 NOT NULL,
          [DataAlteracao] datetime2 NOT NULL,
          [Tipo] nvarchar(13) NOT NULL,
          CONSTRAINT [PK_Lancamento] PRIMARY KEY ([Id]),
          CONSTRAINT [FK_Lancamento_ContaBancaria_ContaBancariaId] FOREIGN KEY ([ContaBancariaId]) REFERENCES [ContaBancaria] ([Id]) ON DELETE CASCADE
      );
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE INDEX [IX_ContaBancaria_ClienteId] ON [ContaBancaria] ([ClienteId]);
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      CREATE INDEX [IX_Lancamento_ContaBancariaId] ON [Lancamento] ([ContaBancariaId]);
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (4ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
      VALUES (N'20260905015834_InicialModeloAtual', N'8.0.0');
Done.
PS C:\Amauri\GitHub\Banco>




--Alterações

1. Compilar
dotnet build Banco.Dominio\Banco.sln


2. Verificar alteração pendente
dotnet ef migrations has-pending-model-changes --project Banco.Infraestrutura --startup-project Banco.Api
Deve aparecer: Changes have been made to the model since the last migration.

3. Criar a migration
dotnet ef migrations add Cliente-RenomearDataAlteracaoTemporariaAmauri --project Banco.Infraestrutura --startup-project Banco.Api --output-dir Migrations




PS C:\Amauri\GitHub\Banco> dotnet build Banco.Dominio\Banco.sln
Restauração concluída (0,5s)
  Banco.Dominio net8.0 êxito (0,1s) → Banco.Dominio\bin\Debug\net8.0\Banco.Dominio.dll
  Banco.Aplicacao net8.0 êxito (0,1s) → Banco.Aplicacao\bin\Debug\net8.0\Banco.Aplicacao.dll
  Banco.Console net8.0 êxito (0,1s) → Banco.Console\bin\Debug\net8.0\Banco.Console.dll
  Banco.Infraestrutura net8.0 êxito (0,1s) → Banco.Infraestrutura\bin\Debug\net8.0\Banco.Infraestrutura.dll
  Banco.Api net8.0 êxito (0,4s) → Banco.Api\bin\Debug\net8.0\Banco.Api.dll

Construir êxito em 1,4s
PS C:\Amauri\GitHub\Banco> dotnet ef migrations has-pending-model-changes --project Banco.Infraestrutura --startup-project Banco.Api
Build started...
Build succeeded.
Changes have been made to the model since the last migration. Add a new migration.
PS C:\Amauri\GitHub\Banco> dotnet ef migrations add Cliente-RenomearDataAlteracaoTemporariaAmauri --project Banco.Infraestrutura --startup-project Banco.Api --output-dir Migrations
Build started...
Build succeeded.
Done. To undo this action, use 'ef migrations remove'





 * 
 */

