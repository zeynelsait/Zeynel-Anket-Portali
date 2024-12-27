using AnketPortali.Models;
using AnketPortali.Repositories;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using AutoMapper;
using SurveyApp.Repositories;
using AnketPortali.ViewModels.Survey;
using test=AnketPortali.ViewModels.Survey;
using AnketPortali.ViewModels.Take;
using AnketPortali.ViewModels;
using AnketPortali.ViewModel;
using AspNetCoreHero.ToastNotification.Abstractions;

namespace AnketPortali.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SurveyRepository _surveyRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly INotyfService _notfy;

        public HomeController(
            ILogger<HomeController> logger,
            SurveyRepository surveyRepository,
            CategoryRepository categoryRepository,
            IMapper mapper,
            INotyfService notyfService)
        {
            _logger = logger;
            _surveyRepository = surveyRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _notfy= notyfService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var surveys = await _surveyRepository.GetAllWithCategoryAsync();

            var viewModel = new HomeViewModel
            {
                Categories = _mapper.Map<List<CategoryViewModel>>(categories),
                Surveys = _mapper.Map<List<SurveyViewModel>>(surveys.Where(s => s.IsActive))
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Take(int id)
        {
            var survey = await _surveyRepository.GetByIdWithQuestionsAsync(id);
            if (survey == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userId == null)
            {
                _notfy.Warning("Ankete katılmak için giriş yapmalısınız");
                return RedirectToAction("Login", "User");
            }   
            // Kullanıcının bu ankete daha önce katılıp katılmadığını kontrol et
            var hasParticipated = await _surveyRepository.HasUserParticipatedInSurvey(id, userId);
            if (hasParticipated)
            {
                _notfy.Warning("Bu Ankete Katıldınız");
                return RedirectToAction("Index", "Home");
            }


            // Session'da soru indeksini tut
            HttpContext.Session.SetInt32($"Survey_{id}_Index", 0);
            var firstQuestion = survey.Questions.FirstOrDefault();

            var viewModel = new SurveyTakeViewModel
            {
                SurveyId = survey.Id,
                SurveyTitle = survey.Title,
                Description = survey.Description,
                CurrentQuestionIndex = 0,
                TotalQuestions = survey.Questions.Count,
                CurrentQuestion = firstQuestion != null ?
                    _mapper.Map<QuestionTakeViewModel>(firstQuestion) : null
            };
            return View(viewModel);

        }
  
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer([FromBody] SurveyAnswerViewModel answer)
        {
            if (!ModelState.IsValid) return Json(new { success = false });
            var survey = await _surveyRepository.GetByIdWithQuestionsAsync(answer.SurveyId);
            if (survey == null) return Json(new { success = false });
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _surveyRepository.SaveAnswerAsync(new SurveyResponse
            {
                SurveyId = answer.SurveyId,
                QuestionId = answer.QuestionId,
                SelectedOptionId = answer.SelectedOptionId,
                UserId = userId,
                ResponseDate = DateTime.Now
            });
            // Sonraki soru indeksini al
            var currentIndex = HttpContext.Session.GetInt32($"Survey_{answer.SurveyId}_Index") ?? 0;
            var nextIndex = currentIndex + 1;
            // Son soru kontrolü
            if (nextIndex >= survey.Questions.Count)
            {
                return Json(new
                {
                    success = true,
                    completed = true
                });
            }
            // Session'da indeksi güncelle
            HttpContext.Session.SetInt32($"Survey_{answer.SurveyId}_Index", nextIndex);
            // Sonraki soruyu döndür
            var nextQuestion = survey.Questions.ElementAt(nextIndex);
            var mappedQuestion = _mapper.Map<QuestionTakeViewModel>(nextQuestion);
            return Json(new
            {
                success = true,
                completed = false,
                nextQuestion = mappedQuestion,
                currentIndex = nextIndex,
                totalQuestions = survey.Questions.Count
            });
        }
        public IActionResult Privacy()
        {
            return View();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
