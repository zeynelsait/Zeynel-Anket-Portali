using AnketPortali.ViewModels.Survey;
using AnketPortali.ViewModels.Question;
using AnketPortali.ViewModels.Take;
namespace AnketPortali.ViewModels.Survey
{
    public class SurveyQuestionsViewModel
    {
        public int SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public List<QuestionViewModel> Questions { get; set; } = new List<QuestionViewModel>();
    }
} 