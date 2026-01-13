
using System.Threading;
// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

//How to use class method interface

IDriveable myCar =  new Car();
myCar.brand = "Toyota";
myCar.price = 10000;
Console.WriteLine(myCar.brand);
Console.WriteLine(myCar.price);
myCar.Drive();

Motorcycle myMotor =  new Motorcycle();
myMotor.brand = "Honda";
myMotor.price = 1000;
Console.WriteLine(myMotor.brand);
Console.WriteLine(myMotor.price);
myMotor.Ride();

//Encapsulation
BankAccount myBankAccount = new BankAccount();
myBankAccount.Deposit(10010);
Console.WriteLine(myBankAccount.GetBalance());

//Inheritance and Polymorphism
myCar.Move();
myMotor.Move();

Vehicle v;
v = new Car();
v.Move();

v = new Motorcycle();
v.Move();

v = new Boat();
v.Move();

//Simply Class
Mahasiswa mhs = new Mahasiswa();
mhs.Nama = "Andi";
mhs.NIM = "12345";
mhs.TampilkanData();

//Constructor
Produk myProduct = new Produk("Baju", 100);
Console.WriteLine(myProduct.Nama);
Console.WriteLine(myProduct.Harga);

//Basic Syntax
// How to use Keyword as identifier use "@" example using,Int,class,public
int @using = 123;
Console.WriteLine(@using);
// Contextual keyword bisa menjadi keyword atau identifier tergantung konteks. Example var,async,await,get,set
int var = 123;
Console.WriteLine(var);
//Literal adalah nilai langsung di dalam kode. example 10,3.14,"Hello",true


// Ini komentar
/* Ini komentar
   lebih dari satu baris */

//Custom Types - Selain tipe bawaan, kita bisa membuat tipe sendiri dengan class.
UnitConverter feetToInches = new UnitConverter(12);
Console.WriteLine(feetToInches.Convert(30)); // 360

char omega = '\u03A9'; // Ω
Console.WriteLine(omega);

//MultiDimension Array
        // Membuat array 2 dimensi (3 baris, 4 kolom)
        int[,] nilai = new int[3, 5];

        // Mengisi array
        for (int i = 0; i < nilai.GetLength(0); i++) // baris
        {
            for (int j = 0; j < nilai.GetLength(1); j++) // kolom
            {
                nilai[i, j] = (i + 1) * 10 + j;
            }
        }

        // Menampilkan isi array
        for (int i = 0; i < nilai.GetLength(0); i++)
        {
            for (int j = 0; j < nilai.GetLength(1); j++)
            {
                Console.Write(nilai[i, j] + "\t");
            }
            Console.WriteLine();
        }
        Console.WriteLine(nilai[1,2]);
        Console.WriteLine(nilai[0,0]);

int[,] matriks =
{
    { 1, 2, 3 },
    { 4, 5, 6 },
    { 7, 8, 9 }
};

Console.WriteLine(matriks[1, 2]); // Output: 6

//Lock example
    static int counter = 0;
    static readonly object _lock = new object();

    static void Increment()
    {
        for (int i = 0; i < 1000; i++)
        {
            lock (_lock)
            {
                counter++;
            }
        }
    }

    static void Main()
    {
        Thread t1 = new Thread(Increment);
        Thread t2 = new Thread(Increment);

        t1.Start();
        t2.Start();

        t1.Join();
        t2.Join();

        Console.WriteLine(counter); // Pasti 2000
    }
