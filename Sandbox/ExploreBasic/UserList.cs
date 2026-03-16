public class UserList
{
    public List<string> Users { get; private set; }

    public UserList(List<string> users)
    {
        Users = users;
    }

    public void AddUser(string user)
    {
        if (!string.IsNullOrEmpty(user))
            Users.Add(user);
    }
    public List<string> GetUsers()
    {
        return Users;
    }
}
