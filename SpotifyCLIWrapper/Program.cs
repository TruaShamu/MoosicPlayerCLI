using Spectre.Console;
using Spectre.Console.Cli;

namespace MyApp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            AnsiConsole.ResetDecoration();
            AnsiConsole.Write(new Markup("Test\n", new Style(decoration: (Decoration.Underline))));
        }
    }
}