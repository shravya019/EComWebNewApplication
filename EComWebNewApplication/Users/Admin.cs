namespace EComWebNewApplication.Users
{
    public class Admin : User
    {
        public override void DisplayUserInfo()
        {
            Console.WriteLine($"Admin: {Username}, Role: {Role}");
        }

        public void ManageInventory()
        {
            Console.WriteLine($"{Username} is managing inventory...");
        }
    }
}
