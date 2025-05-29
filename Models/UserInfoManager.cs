namespace QMS.Models
{
    public class UserInfoManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserInfoManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession Session => _httpContextAccessor.HttpContext.Session;

        public string UserID
        {
            get => Session.GetString("UserID");
            set => Session.SetString("UserID", value);
        }

        public string UserType
        {
            get => Session.GetString("UserTypeDesignation");
            set => Session.SetString("UserTypeDesignation", value);
        }

        public string UserName
        {
            get => Session.GetString("UserName");
            set => Session.SetString("UserName", value);
        }

        public string IsActive
        {
            get => Session.GetString("IsActive");
            set => Session.SetString("IsActive", value);
        }

        public string LocationID
        {
            get => Session.GetString("LocationID");
            set => Session.SetString("LocationID", value);
        }

        public string AccountID
        {
            get => Session.GetString("AccountID");
            set => Session.SetString("AccountID", value);
        }

        public string Dnycon
        {
            get => Session.GetString("Dnycon");
            set => Session.SetString("Dnycon", value);
        }
    }
}
