// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, AnharAhya!");

Console.Write("Masukan Angka : ");
string input = Console.ReadLine();

int n =int.Parse(input);

Console.WriteLine("Angka anda adalah "+n);

for (int x = 1 ; x <=n ; x++)
{
    if (x % 3 == 0 && x % 5 == 0 && x % 7 == 0)
        Console.Write("foobarjazz");
    
    else if (x % 3 == 0 && x % 7 == 0)
        Console.Write("foojazz");
    
    else if (x % 5 == 0 && x % 7 == 0)
        Console.Write("barjazz");
    
     else if (x % 7 == 0)
        Console.Write("jazz");                               

    else if (x % 3 == 0 && x % 5 == 0)
        Console.Write("foobar");
    
    else if(x % 3 == 0) 
        Console.Write("foo");   
    
    else if(x % 5 == 0) 
        Console.Write("bar");   
    
    else
    
        Console.Write(x);
    
    if (x < n)
    
        Console.Write(", ");
    
    
}





