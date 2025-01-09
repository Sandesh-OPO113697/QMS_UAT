namespace QMS.Models
{
    public class FeatureWithSubFeatures
    {
        public string FeatureValue { get; set; }
        public string FeatureName { get; set; }
        public List<SubFeature> SubFeatures { get; set; }
    }
    public class SubFeature
    {
        public string SubFeatureValue { get; set; }
        public string SubFeatureName { get; set; }
    }
}
