using System.Dynamic;

public class UserClass
{
    public string name {get;set;}
    public string userId {get;set;}
    public string gender{get;set;}

    public void GetUser(string name, string userId, string gender)
    {
        string Name = name;
        string UserId = userId;
        string Gender = gender;
        Console.WriteLine($"{Name}, {UserId}, {Gender}");
    }
}

