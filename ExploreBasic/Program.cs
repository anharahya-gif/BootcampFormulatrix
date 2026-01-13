
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

