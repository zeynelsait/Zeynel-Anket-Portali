using AnketPortali.Models;
using AnketPortali.ViewModels.Survey;
using AnketPortali.ViewModels.Question;
using AnketPortali.ViewModels.Take;
using AutoMapper;
using AnketPortali.ViewModel;

namespace AnketPortali.Mapping
{
    public class MapProfile : Profile
    {
        public MapProfile()
        {
            CreateMap<Category, CategoryViewModel>().ReverseMap();
            CreateMap<AppUser,LoginViewModel>().ReverseMap();
            CreateMap<AppUser,RegisterViewModel>().ReverseMap();

            // Survey mappings
            CreateMap<Survey, SurveyViewModel>()
                .ForMember(dest => dest.CategoryName, 
                    opt => opt.MapFrom(src => src.Category.Name));
            
            // Survey -> SurveyViewModel ve tersi için mapping
            CreateMap<SurveyViewModel, Survey>()
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.Questions, opt => opt.Ignore());

            // Question mappings
            CreateMap<SurveyQuestion, QuestionViewModel>()
                .ForMember(dest => dest.Options, 
                    opt => opt.MapFrom(src => src.Options));

            CreateMap<SurveyOption, OptionViewModel>();

            // Take mappings
            CreateMap<Survey, SurveyTakeViewModel>()
                .ForMember(dest => dest.CurrentQuestionIndex, opt => opt.Ignore())
                .ForMember(dest => dest.TotalQuestions, 
                    opt => opt.MapFrom(src => src.Questions.Count))
                .ForMember(dest => dest.CurrentQuestion, opt => opt.Ignore());

            CreateMap<SurveyQuestion, QuestionTakeViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.QuestionText))
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

            CreateMap<SurveyOption, OptionTakeViewModel>();

            // Questions list mapping
            CreateMap<Survey, SurveyQuestionsViewModel>()
                .ForMember(dest => dest.Questions, 
                    opt => opt.MapFrom(src => src.Questions));
        }
    }
}
