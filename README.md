# Banco

Objetivo

Observar os pilares da orientação a objetos (abstração, encapsulamento, herança e polimorfismo) interpretando e implementando um domínio de negócio “Agência bancária”.

![image](https://github.com/AmauriDallOglio/Banco/assets/13471113/f3d6c883-edc2-43ab-a119-60788fe9193b)


Domínio

Simulação do contexto de uma agência bancária, fluxo de abertura de conta e utilização da conta aberta.

Referências

DDD e microservices:
https://medium.com/cwi-software/domain-driven-design-do-in%C3%ADcio-ao-c%C3%B3digo-569b23cb3d47
https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/net-core-microservice-domain-model
POCO classes: https://www.eduardopires.net.br/2012/10/classes-poco/
Operadores: https://docs.microsoft.com/pt-br/dotnet/csharp/language-reference/operators/
ValueObject: https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects
CleanCode: https://balta.io/artigos/clean-code

Inicializacao do banco de dados

A API executa a inicializacao do banco no startup pela classe `Banco.Api/Configuracao/Migracao.cs`.

Comportamento:

- Valida se o banco existe e usa migrations do EF Core para criar ou atualizar o esquema.
- Executa `Database.Migrate()` automaticamente quando `Database:AutoMigrate` estiver habilitado.
- Se encontrar tabelas criadas anteriormente por `EnsureCreated()` sem historico em `__EFMigrationsHistory`, registra a migration inicial como baseline para preservar os dados existentes.
- Executa seed inicial somente quando a tabela `Clientes` estiver vazia.
- Alteracoes nao destrutivas geradas por migrations, como novas tabelas e novas colunas, sao aplicadas automaticamente.
- Alteracoes destrutivas detectadas no script, como `DROP TABLE`, `DROP COLUMN`, `ALTER COLUMN` e `TRUNCATE TABLE`, nao sao aplicadas automaticamente. A API gera um script em `Banco.Api/DatabaseScripts` para revisao manual.
- Todas as operacoes registram logs detalhados com sucesso, avisos e erros.

Configuracao:

```json
"Database": {
  "AutoMigrate": true,
  "Reinstall": false
}
```

`AutoMigrate` controla se a API aplica migrations ao iniciar.
`Reinstall` remove e recria o banco antes de migrar. Use apenas em ambiente local/desenvolvimento, pois apaga dados.

Execucao:

```powershell
dotnet build Banco.Dominio\Banco.sln
dotnet run --project Banco.Api\Banco.Api.csproj
```

Para criar novas migrations:

```powershell
dotnet ef migrations add NomeDaMigration --project Banco.Infraestrutura\Banco.Infraestrutura.csproj --startup-project Banco.Api\Banco.Api.csproj --context BancoContexto --output-dir Migrations
```

Pilares da Orientação a Objetos

Abstração

Representação de objetos reais
Simplifica um problema difícil, dividindo em partes menores
Partes independentes com responsabilidade definida
Reaproveitamento de código

Encapsulamento

Ocultar detalhes internos das partes
Visualização do objeto como uma caixa preta
Você sabe o que faz (Interface)
Não sabe como ela faz (Implementação)

Herança

Criar nova classe a partir de uma existente
Herda atributos
Herda comportamentos
Herda implementações

Polimorfismo

Polimorfismo – muitas formas
Único nome – diferentes comportamentos

“Abrir”
Uma porta
Uma caixa
Uma janela
Uma conta bancária
