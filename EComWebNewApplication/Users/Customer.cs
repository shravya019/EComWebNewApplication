namespace EComWebNewApplication.Users
{
    public class Customer : User
    {
        public override void DisplayUserInfo()
        {
            Console.WriteLine($"Customer: {Username}, Role: {Role}");
        }

        public void PlaceOrder()
        {
            Console.WriteLine($"{Username} is placing an order");
        }
    }
}
