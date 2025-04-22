namespace EComWebNewApplication.Users
{
   
        public abstract class User
        {
            public int UserId { get; set; }
            public required string Username { get; set; }
            public required string Role { get; set; }

            public abstract void DisplayUserInfo();
        }
    }

