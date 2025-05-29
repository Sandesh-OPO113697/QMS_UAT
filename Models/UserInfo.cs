namespace QMS.Models
{
    public static class UserInfo
    {
        //private static IHttpContextAccessor _httpContextAccessor;

        //public static void Configure(IHttpContextAccessor accessor)
        //{
        //    _httpContextAccessor = accessor;
        //}

        //private static ISession Session
        //{
        //    get
        //    {
        //        var session = _httpContextAccessor?.HttpContext?.Session;
        //        if (session == null)
        //            throw new Exception("Session is not available. Make sure it's enabled.");
        //        return session;
        //    }
        //}

        //public static string UserID
        //{
        //    get => Session.GetString("UserID");
        //    set => Session.SetString("UserID", value);
        //}

        //public static string UserType
        //{
        //    get => Session.GetString("UserTypeSession");
        //    set => Session.SetString("UserTUserTypeSessionype", value);
        //}

        //public static string UserName
        //{
        //    get => Session.GetString("UserNameSession");
        //    set => Session.SetString("UserNameSession", value);
        //}

        //public static string IsActive
        //{
        //    get => Session.GetString("IsActiveSession");
        //    set => Session.SetString("IsActIsActiveSessionive", value);
        //}

        //public static string LocationID
        //{
        //    get => Session.GetString("LocationIDSession");
        //    set => Session.SetString("LocationIDSession", value);
        //}

        //public static string AccountID
        //{
        //    get => Session.GetString("AccountIDSession");
        //    set => Session.SetString("AccountIDSession", value);
        //}

        //public static string Dnycon
        //{
        //    get => Session.GetString("DnyconSession");
        //    set => Session.SetString("DnyconSession", value);
        //}
        public static string UserID { get; set; }
        public static string UserType { get; set; }
        public static string UserName { get; set; }
        public static string IsActive { get; set; }
        public static string LocationID { get; set; }
        public static string AccountID { get; set; }
        public static string Dnycon { get; set; }
    }
}
