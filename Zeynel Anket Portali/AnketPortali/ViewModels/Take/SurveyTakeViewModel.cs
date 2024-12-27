namespace AnketPortali.ViewModels.Take
{
    public class SurveyTakeViewModel
    {
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public string Description { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public int TotalQuestions { get; set; }
        public QuestionTakeViewModel CurrentQuestion { get; set; }
    }
} 