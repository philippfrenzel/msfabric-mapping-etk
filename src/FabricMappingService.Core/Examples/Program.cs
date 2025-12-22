using FabricMappingService.Core.Examples;

namespace FabricMappingService.Core.Examples;

/// <summary>
/// Console application to demonstrate reference table functionality.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== Fabric Mapping Service - Reference Table Examples ===\n");

        Console.WriteLine("--- Scenario 1: Manual Reference Table ---");
        ReferenceTableExample.ManualReferenceTableExample();

        Console.WriteLine("\n\n--- Scenario 2: Automated Reference Table from Source Data ---");
        ReferenceTableExample.AutomatedReferenceTableExample();

        Console.WriteLine("\n\n--- Multiple Reference Tables ---");
        ReferenceTableExample.MultipleTablesExample();

        Console.WriteLine("\n\n=== All examples completed successfully! ===");
    }
}
