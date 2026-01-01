using System.Text;

namespace Lithium.Server.Core.Commands;

internal static class CommandLineTokenizer
{
    public static string[] Tokenize(string input)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var c in input)
        {
            if (c is '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }

                continue;
            }

            current.Append(c);
        }

        if (current.Length > 0)
            result.Add(current.ToString());

        if (!inQuotes) 
            return result.ToArray();
        
        Console.WriteLine("Unclosed quote in command line");
        return [];
    }
}