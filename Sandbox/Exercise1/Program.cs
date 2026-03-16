//Console.WriteLine("Hello, AnharAhya!");

internal class Program
{
    private static void Main(string[] args)
    {
        Console.Write("Masukan Angka : ");
        string input = Console.ReadLine();

        int n = int.Parse(input);

        Console.WriteLine("Angka anda adalah " + n);

        // for (int x = 1; x <= n; x++)
        // {
        //     if (x % 3 == 0 && x % 5 == 0 && x % 7 == 0 && x % 9 == 0)
        //         Console.Write("foobarjazzhuzz");

        //     else if (x % 5 == 0 && x % 7 == 0 && x % 9 == 0)
        //         Console.Write("barjazzhuzz");

        //     else if (x % 3 == 0 && x % 7 == 0 && x % 9 == 0)
        //         Console.Write("foojazzhuzz");

        //     else if (x % 3 == 0 && x % 5 == 0 && x % 9 == 0)
        //         Console.Write("foobarhuzz");

        //     else if (x % 3 == 0 && x % 5 == 0 && x % 7 == 0)
        //         Console.Write("foobarjazz");

        //     else if (x % 3 == 0 && x % 7 == 0)
        //         Console.Write("foojazz");

        //     else if (x % 5 == 0 && x % 7 == 0)
        //         Console.Write("barjazz");

        //     else if (x % 3 == 0 && x % 5 == 0)
        //         Console.Write("foobar");

        //     else if (x % 5 == 0 && x % 9 == 0)
        //         Console.Write("barhuzz");

        //     else if (x % 7 == 0 && x % 9 == 0)
        //         Console.Write("jazzhuzz");

        //     else if (x % 3 == 0 && x % 9 == 0)
        //         Console.Write("foohuzz");

        //     else if (x % 9 == 0)
        //         Console.Write("huzz");

        //     else if (x % 7 == 0)
        //         Console.Write("jazz");

        //     else if (x % 3 == 0)
        //         Console.Write("foo");

        //     else if (x % 5 == 0)
        //         Console.Write("bar");

        //     else

        //         Console.Write(x);

        //     if (x < n)

        //         Console.Write(", ");


        // }


        // for (int x = 1; x <= n; x++)
        // {
        //     string output = "";

        //     if (x % 3 == 0) output += "foo";
        //     if (x % 5 == 0) output += "bar";
        //     if (x % 7 == 0) output += "jazz";
        //     if (x % 9 == 0) output += "huzz";

        //     if (output == "")
        //         output = x.ToString();

        //     Console.Write(output);

        //     if (x < n)
        //         Console.Write(", ");
        // }
         var myClass = new DivisibleWordGenerator();

        
        myClass.AddRule(3, "foo");
        myClass.AddRule(4, "baz");
        myClass.AddRule(5, "sus");
        myClass.AddRule(7, "jazz");
        myClass.AddRule(9, "huzz");

        myClass.Generate(n);
    }
}