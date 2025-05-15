namespace QMS.Models
{
    public class QuestionViewModel
    {
        //public int QuestionId { get; set; }
        //public string QuestionText { get; set; }
        //public string AnswerType { get; set; }
        //public List<AnswerOptionViewModel> Options { get; set; }

        //// For capturing user response
        //public int? SelectedOptionId { get; set; }

        //public int QuestionId { get; set; }
        //public string QuestionText { get; set; }
        //public string AnswerType { get; set; }
        //public List<AnswerOptionViewModel> Options { get; set; }

        //// For capturing user response
        //public int? SelectedOptionId { get; set; }  // For single choice (radio, dropdown)
        //public List<int> SelectedOptionIds { get; set; }

        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public string AnswerType { get; set; }
        public List<AnswerOptionViewModel> Options { get; set; }

        // For single choice questions (radio, dropdown, textbox)
        public int? SelectedOptionId { get; set; }

        // For multiple choice questions (checkbox, multi-select)
        public List<int> SelectedOptionIds { get; set; } = new List<int>();

        // For textbox answers (optional)
        public string TextAnswer { get; set; }

    }
}
