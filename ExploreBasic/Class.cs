public interface IDriveable
    {
        public string brand {get;set;}
        public int price {get;set;}

        void Drive();
        void Move();
    }
 public interface IRideable
    {
        void Ride();
    }
public class Car : Vehicle, IDriveable
{
   public string brand {get;set;}
   public int price {get;set;}

   public void Drive()
    {
        Console.WriteLine("Mobil Berjalan.");
    } 
      public override void Move()
    {
        Console.WriteLine("Mobil jalan");
    }
}
public class Motorcycle : Vehicle
{
   public string brand {get;set;}
   public int price {get;set;}

   public void Ride()
    {
        Console.WriteLine("Motor Berjalan.");
    } 
      public override void Move()
    {
        Console.WriteLine("Motor jalan");
    }
}

public class BankAccount
{
    private int _balance;

    public void Deposit(int amount)
    {
        _balance += amount;
    }

    public int GetBalance()
    {
        return _balance;
    }
}

public class Vehicle
{
    public virtual void Move()
    {
        Console.WriteLine("Kendaraan bergerak");
    }
}

public class Boat : Vehicle
{
}

class Mahasiswa
{
    public string Nama { get; set; }
    public string NIM { get; set; }

    public void TampilkanData()
    {
        Console.WriteLine($"Nama: {Nama}, NIM: {NIM}");
    }
}



class Produk
{
    public string Nama { get; set; }
    public int Harga { get; set; }

    public Produk(string nama, int harga)
    {
        Nama = nama;
        Harga = harga;
    }
}

public class UnitConverter
{
    int ratio;

    public UnitConverter(int unitRatio)
    {
        ratio = unitRatio;
    }

    public int Convert(int unit)
    {
        return unit * ratio;
    }
}

public class Counter
{
    private static int counter = 0;
    private static readonly object _lock = new object();

    public static void Increment()
    {
        for (int i = 0; i < 1000; i++)
        {
            lock (_lock)
            {
                counter++;
            }
        }
    }

    public static int GetValue()
    {
        return counter;
    }
}

