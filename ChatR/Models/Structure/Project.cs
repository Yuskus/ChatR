namespace ChatR.Models.Structure;

public static class Routes
{
    public static class Pages
    {
        public static class Auth
        {
            public const string Login = "/Auth/Login"; //Routes.Pages.Auth.Login
            public const string Logout = "/Auth/Logout";  //Routes.Pages.Auth.Logout
            public const string Register = "/Auth/Register";  //Routes.Pages.Auth.Register
        }

        public static class Chat
        {
            public const string Room = "/Chat/Room"; //Routes.Pages.Chat.Room
        }

        public static class Users
        {
            public const string Profile = "/Users/Profile"; //Routes.Pages.Users.Profile
        }

        public const string Error = "/Error"; //Routes.Pages.Error
        public const string Index = "/Index"; //Routes.Pages.Index
    }
}
