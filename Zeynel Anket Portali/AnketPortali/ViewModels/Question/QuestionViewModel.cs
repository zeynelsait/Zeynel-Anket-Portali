using System.ComponentModel.DataAnnotations;

namespace AnketPortali.ViewModels.Question
{
    public class QuestionViewModel
    {
        public int Id { get; set; }
        public int SurveyId { get; set; }
        public string QuestionText { get; set; }
        public List<OptionViewModel> Options { get; set; } = new List<OptionViewModel>();

        // Soru ekleme için
        [Required(ErrorMessage = "Soru metni zorunludur")]
        public string NewQuestionText { get; set; }

        [Required(ErrorMessage = "En az 2 seçenek eklenmelidir")]
        [MinLength(2, ErrorMessage = "En az 2 seçenek eklenmelidir")]
        public List<string> NewOptions { get; set; } = new List<string>();
    }
} 