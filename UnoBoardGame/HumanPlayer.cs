using System;

public class HumanPlayer : Player
{
    public HumanPlayer(string name) : base(name) { }

    public override Card TakeTurn(Card topCard, Deck deck)
    {
        Console.WriteLine($"\nGiliran: {Name}");
        Console.WriteLine($"Top Card: {topCard}");

        for (int i = 0; i < Hand.Count; i++)
            Console.WriteLine($"{i}. {Hand[i]}");
        int choice;
        while (true)
        {
            Console.Write("Pilih kartu (-1 untuk draw): ");
            string input = Console.ReadLine();

            if (!int.TryParse(input, out choice))
            {
                Console.WriteLine("Input harus angka!");
                continue;
            }

            if (choice == -1)
                break;

            if (choice < 0 || choice >= Hand.Count)
            {
                Console.WriteLine("Pilihan tidak valid!");
                continue;
            }

            break;
        }


        if (choice == -1)
        {
            DrawCard(deck);
            return null;
        }

        var card = Hand[choice];
        if (!card.CanPlayOn(topCard))
        {
            Console.WriteLine("Tidak bisa dimainkan!");
            return null;
        }

        Hand.Remove(card);

        // 👉 CEK UNO
        if (Hand.Count == 1)
        {
            Console.Write("Ketik 'UNO' : ");
            string input = Console.ReadLine();

            if (input.ToUpper() == "UNO")
                CallUno();
        }

        return card;
    }
}
