using System;
using System.Collections.Generic;

class Player
{
    public string Name;
    public int Money = 1000;
    public int Position = 0;

    public Player(string name)
    {
        Name = name;
    }
}

class Tile
{
    public string Name;
    public int Price;
    public int BaseRent;
    public int Level = 0;
    public Player Owner;

    public Tile(string name, int price, int rent)
    {
        Name = name;
        Price = price;
        BaseRent = rent;
    }

    public int Rent => BaseRent * (Level + 1);

    public int UpgradeCost => Price / 2 * (Level + 1);

    public bool CanUpgrade()
    {
        return Level < 3;
    }

    public void Upgrade()
    {
        Level++;
    }
}


class MiniMonopoly
{
    static List<Tile> board = new List<Tile>();
    static Player[] players;
    static Random rng = new Random();

    static void Main()
    {
        SetupBoard();

        players = new Player[]
        {
            new Player("Player 1"),
            new Player("Player 2")
        };

        int turn = 0;

        while (true)
        {
            Player p = players[turn % players.Length];
            Console.Clear();

            Console.WriteLine($"🎲 Giliran {p.Name}");
            Console.WriteLine($"💰 Uang: {p.Money}");
            Console.WriteLine("Tekan ENTER untuk lempar dadu...");
            Console.ReadLine();

            int dice = rng.Next(1, 7);
            Console.WriteLine($"Dadu: {dice}");

            int oldPos = p.Position;
            p.Position = (p.Position + dice) % board.Count;

            if (p.Position < oldPos)
            {
                p.Money += 200;
                Console.WriteLine("Lewat START → +200");
            }

            Tile tile = board[p.Position];
            Console.WriteLine($"Berhenti di {tile.Name}");

            HandleTile(p, tile);

            if (p.Money <= 0)
            {
                Console.WriteLine($"{p.Name} BANGKRUT!");
                Console.WriteLine($"🏆 {players[(turn + 1) % 2].Name} MENANG!");
                break;
            }

            Console.WriteLine("Tekan ENTER...");
            Console.ReadLine();
            turn++;
        }
    }

    static void SetupBoard()
    {
        board.Add(new Tile("START", 0, 0));
        board.Add(new Tile("Tokyo", 200, 50));
        board.Add(new Tile("Seoul", 220, 55));
        board.Add(new Tile("Paris", 240, 60));
        board.Add(new Tile("London", 260, 65));
        board.Add(new Tile("Berlin", 280, 70));
        board.Add(new Tile("Rome", 300, 75));
        board.Add(new Tile("Madrid", 320, 80));
        board.Add(new Tile("Lisbon", 340, 85));
        board.Add(new Tile("New York", 360, 90));
    }

    static void HandleTile(Player p, Tile t)
    {
        if (t.Price == 0) return;

        if (t.Owner == null)
        {
            if (p.Money >= t.Price)
            {
                Console.WriteLine($"Beli {t.Name} seharga {t.Price}? (y/n)");
                if (Console.ReadLine().ToLower() == "y")
                {
                    p.Money -= t.Price;
                    t.Owner = p;
                    Console.WriteLine("🏠 Properti dibeli!");
                }
            }
        }
        else if (t.Owner == p)
        {
            Console.WriteLine($"Ini properti kamu (Level {t.Level})");
            Console.WriteLine($"Sewa sekarang: {t.Rent}");

            if (t.CanUpgrade() && p.Money >= t.UpgradeCost)
            {
                Console.WriteLine($"Upgrade ke level {t.Level + 1}? (Biaya {t.UpgradeCost}) y/n");
                if (Console.ReadLine().ToLower() == "y")
                {
                    p.Money -= t.UpgradeCost;
                    t.Upgrade();
                    Console.WriteLine("⬆️ Properti di-upgrade!");
                }
            }
        }
        else
        {
            Console.WriteLine($"Bayar sewa {t.Rent} ke {t.Owner.Name}");
            p.Money -= t.Rent;
            t.Owner.Money += t.Rent;
        }
    }

}
