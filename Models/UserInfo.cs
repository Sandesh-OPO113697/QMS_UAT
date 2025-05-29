namespace QMS.Models
{
    public static class UserInfo
    {

        //public static string UserID { get; set; }
        //public static string UserType { get; set; }
        //public static string UserName { get; set; }
        //public static string IsActive { get; set; }
        //public static string LocationID { get; set; }
        //public static string AccountID { get; set; }
        //public static string Dnycon { get; set; }

        private static IHttpContextAccessor _contextAccessor;

        public static void Configure(IHttpContextAccessor accessor)
        {
            _contextAccessor = accessor;
        }
        private static ISession Session => _contextAccessor.HttpContext.Session;

        public static string UserID => Session.GetString("UserID");
        public static string UserType => Session.GetString("UserTypeDesignation");
        public static string UserName => Session.GetString("UserName");
        public static string IsActive => Session.GetString("IsActive");
        public static string LocationID => Session.GetString("LocationID");
        public static string AccountID => Session.GetString("AccountID");
        public static string Dnycon => Session.GetString("Dnycon");

    }
}
