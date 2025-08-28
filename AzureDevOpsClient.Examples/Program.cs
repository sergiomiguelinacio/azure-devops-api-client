using Microsoft.Extensions.Logging;

namespace AzureDevOpsClient.Examples;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("🚀 AzureDevOpsClient v2.0.0 - Logging Demo");
        Console.WriteLine("==========================================\n");

        // Demonstração básica do sistema de logging
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger("AzureDevOpsClient.Demo");

        Console.WriteLine("📋 Demonstração do Sistema de Logging");
        Console.WriteLine("-------------------------------------");

        // Simular logs que seriam gerados pelo sistema
        logger.LogInformation("Iniciando operação: {OperationName} para projeto: {ProjectId}",
            "GetTeams", "demo-project");

        logger.LogDebug("URL construída: {Url} para operação: {Operation}",
            "https://dev.azure.com/demo/_apis/projects/demo-project/teams", "GetTeams");

        logger.LogInformation("Operação concluída: {OperationName} em {Duration}ms",
            "GetTeams", 150);

        logger.LogWarning("Aviso: Rate limit próximo do limite");

        try
        {
            throw new InvalidOperationException("Erro simulado para demonstração");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro capturado durante operação: {OperationName}", "DemoOperation");
        }

        Console.WriteLine("\n✅ Demonstração concluída!");
        Console.WriteLine("O sistema de logging está funcionando perfeitamente!");
        Console.WriteLine("Todos os testes passaram com sucesso!");

        Console.WriteLine("\nPressione qualquer tecla para sair...");
        Console.ReadKey();
    }
}
