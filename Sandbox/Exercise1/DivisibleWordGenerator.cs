using System;
using System.Collections.Generic;

public class DivisibleWordGenerator
{
    private readonly Dictionary<int, string> _rules = new();

    public void AddRule(int input, string output)
    {
        if (!_rules.ContainsKey(input))
        {
            _rules.Add(input, output);
        }
    }

    public void Generate(int n)
    {
        for (int i = 1; i <= n; i++)
        {
            string result = "";

            foreach (var rule in _rules)
            {
                if (i % rule.Key == 0)
                {
                    result += rule.Value;
                }
            }

            Console.Write(result == "" ? i.ToString() : result);

            if (i < n)
                Console.Write(", ");
        }

        Console.WriteLine();
    }
}
