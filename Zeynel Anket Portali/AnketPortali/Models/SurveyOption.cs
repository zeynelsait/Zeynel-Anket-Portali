using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AnketPortali.Models
{
    public class SurveyOption
    {
        [Key]
   
        public int Id { get; set; }
        public int? QuestionId { get; set; }
        public string OptionText { get; set; }

        public SurveyQuestion Question { get; set; }
    }
}
