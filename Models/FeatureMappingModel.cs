using System.Reflection;

namespace QMS.Models
{
    public class FeatureMappingModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<string> FeatureList { get; set; }
        public List<string> SubFeatureList { get; set; }


    }
}
