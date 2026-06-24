namespace ChatR.Models.Structure;

public static class Routes
{
    public static class Pages
    {
        public static class Auth
        {
            public const string Login = "/Auth/Login";
            public const string Logout = "/Auth/Logout";
            public const string Register = "/Auth/Register";
        }

        public static class Chat
        {
            public const string Room = "/Chat/Room";
        }

        public static class Users
        {
            public const string Profile = "/Users/Profile";
            public const string EditProfile = "/Users/EditProfile";
        }

        public static class Observings
        {
            public const string UsersFrom = "/Observings/UsersFrom";
            public const string UsersTo = "/Observings/UsersTo";
        }

        public const string Error = "/Error";
        public const string Index = "/Index";
        public const string Info = "/Info";
    }
}
