namespace QMS.Models
{
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public List<Dictionary<string, object>> Modules { get; set; }  // Use Dictionary to represent modules
        public List<object> Features { get; set; }
    }
}
