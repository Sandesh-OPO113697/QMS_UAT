namespace QMS.Models
{
    public class SectionGridModel
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string SectionName { get; set; }
        public int SectionId { get; set; }
        public string RatingName { get; set; }
        public int RatingId { get; set; }
        public string Scorable { get; set; }
        public int Score { get; set; }
        public string Level { get; set; }
        public string Fatal { get; set; }
        public string Active { get; set; }
    }
}
