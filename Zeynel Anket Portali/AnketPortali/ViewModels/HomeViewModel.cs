using AnketPortali.ViewModel;
using AnketPortali.ViewModels.Survey;

namespace AnketPortali.ViewModels
{
    public class HomeViewModel
    {
        public List<CategoryViewModel> Categories { get; set; }
        public List<SurveyViewModel> Surveys { get; set; }
    }
}
