using Banco.Dominio.Entidade;
using Banco.Dominio.Negocio;

class Program
{
    static void Main(string[] args)
    {
        Cliente cliente = null;
        bool sair = false;
        do
        {
            Console.WriteLine("=== Menu ===");
            Console.WriteLine("1 - Cadastrar cliente e abrir conta");
            Console.WriteLine("2 - Realizar operações na conta");
            Console.WriteLine("3 - Realizar operações na conta");
            Console.WriteLine("4 - Teste");
            Console.WriteLine("5 - Sair");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    cliente = CadastrarClienteEConta();
                    break;
                case "2":
                    RealizarOperacoesContaCorrente(cliente);
                    break;
                case "3":
                    RealizarOperacoesContaCorrente(cliente);
                    break;
                case "4":
                    cliente = RealizarOperacoesTeste();
                    break;
                case "5":
                    sair = true;
                    Console.WriteLine("Saindo do programa...");
                    break;
                default:
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    break;
            }
        } while (!sair);
    }

    public static Cliente CadastrarClienteEConta()
    {
        try
        {
            Console.WriteLine("Cadastrando cliente e abrindo conta...");

            // Solicitação de informações do cliente
            Console.WriteLine("Informe os dados do cliente:");
            Console.Write("Nome: ");
            string nome = Console.ReadLine();
            Console.Write("CPF: ");
            string cpf = Console.ReadLine();
            Console.Write("Telefone: ");
            string telefone = Console.ReadLine();
            Console.Write("Endereço (Rua): ");
            string rua = Console.ReadLine();
            Console.Write("CEP: ");
            string cep = Console.ReadLine();
            Console.Write("Cidade: ");
            string cidade = Console.ReadLine();
            Console.Write("Estado: ");
            string estado = Console.ReadLine();

            // Criação do endereço
            Endereco endereco = new Endereco(rua, cep, cidade, estado);

            // Criação do cliente
            Cliente cliente = new Cliente(nome, cpf, telefone, endereco);
            return cliente;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }

    public static void RealizarOperacoesContaCorrente(Cliente cliente)
    {
        try
        {
            Console.WriteLine("Realizando operações na conta corrente...");
            Console.WriteLine("\n Informe os dados da conta:");
            Console.Write("Informe o limite inicial: ");
            decimal saldoInicial = Convert.ToDecimal(Console.ReadLine());

            // Criação da conta corrente
            ContaCorrente contaCorrente = new ContaCorrente(cliente, saldoInicial);

            // Abertura de conta
            Console.Write("\n Digite a senha para abrir a conta: (abc123456789) ");
            string senha = Console.ReadLine();
            contaCorrente.Abrir(senha);

            // Exibe informações da conta
            Console.WriteLine($"\n Conta {contaCorrente.Situacao}: {contaCorrente.NumeroConta}-{contaCorrente.DigitoVerificador}");
            Console.WriteLine(contaCorrente.VerSaldo());

            // Realiza operações na conta
            Console.WriteLine("\n Operações na conta:");
            Console.Write("Valor para sacar: ");
            decimal valorSaque = Convert.ToDecimal(Console.ReadLine());
            contaCorrente.Sacar(valorSaque, senha);
            Console.WriteLine(contaCorrente.VerSaldo());

            Console.Write("\nValor para depositar: ");
            decimal valorDeposito = Convert.ToDecimal(Console.ReadLine());
            contaCorrente.Depositar(valorDeposito);
            Console.WriteLine(contaCorrente.VerSaldo());

            Console.WriteLine("\nExtrato:");
            Console.WriteLine(contaCorrente.VerExtrato());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
 
    public static Cliente RealizarOperacoesTeste()
    {

        try
        {
            // Criação da conta
            Endereco endereco = new Endereco("Rua dos estudos bancarios", "88353-100", "Cidade a", "SC");
            Cliente cliente = new Cliente("Amauri", "000.111.222-33", "87654321", endereco);
            ContaCorrente conta = new ContaCorrente(cliente, 100);



            Console.WriteLine("Conta " + conta.Situacao + ": " + conta.NumeroConta + "-" +
                conta.DigitoVerificador);

            // Abertura de conta
            string senha = "abc123456789";
            conta.Abrir(senha);

            Console.WriteLine("Conta " + conta.Situacao + ": " + conta.NumeroConta + "-" +
                conta.DigitoVerificador);

            // Utilização da conta
            conta.Sacar(10, senha);
            Console.WriteLine(conta.VerSaldo());

            conta.Depositar(50);
            Console.WriteLine(conta.VerSaldo());

            conta.Sacar(20, senha);
            Console.WriteLine(conta.VerSaldo());

            conta.Depositar(10);
            Console.WriteLine(conta.VerExtrato());

            return cliente;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;
        }
    }


}


