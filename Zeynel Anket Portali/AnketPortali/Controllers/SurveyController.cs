using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

using SurveyApp.Repositories;

using Microsoft.AspNetCore.Identity;
using AnketPortali.Repositories;
using AnketPortali.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using AnketPortali.ViewModels.Survey;
using te=AnketPortali.ViewModels.Survey;
using AnketPortali.ViewModels.Take;
using AnketPortali.ViewModels.Question;



namespace SurveyApp.Controllers
{
    [Authorize]
    
    public class SurveyController : Controller
    {
        private readonly SurveyRepository _surveyRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public SurveyController(
            SurveyRepository surveyRepository,
            CategoryRepository categoryRepository,
            IMapper mapper,
            UserManager<AppUser> userManager)
        {
            _surveyRepository = surveyRepository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var surveys = await _surveyRepository.GetAllWithCategoryAsync();
            var viewModel = _mapper.Map<List<SurveyViewModel>>(surveys);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetSurvey(int id)
        {
            var survey = await _surveyRepository.GetByIdWithCategoryAsync(id);
            var viewModel = _mapper.Map<SurveyViewModel>(survey);
            return Json(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(SurveyViewModel model)
        {
          
                var user = await _userManager.GetUserAsync(User);
                var survey = _mapper.Map<Survey>(model);
                survey.CreatedById = user.Id;
                await _surveyRepository.AddAsync(survey);
                ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            return RedirectToAction(nameof(Index));

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var survey = await _surveyRepository.GetByIdWithCategoryAsync(id);
            if (survey == null) return NotFound();

            var viewModel = _mapper.Map<SurveyViewModel>(survey);
            ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SurveyViewModel model)
        {
           
                var survey = await _surveyRepository.GetByIdAsync(model.Id);
                if (survey == null) return NotFound();

                survey.Title = model.Title;
                survey.Description = model.Description;
                survey.CategoryId = model.CategoryId;
                survey.IsActive = model.IsActive;

                await _surveyRepository.UpdateAsync(survey);
                ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "Id", "Name");
                return RedirectToAction(nameof(Index));
            

            
 
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _surveyRepository.DeleteAsync(id);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSurveys()
        {
            var surveys = await _surveyRepository.GetAllWithCategoryAsync();
            var viewModel = _mapper.Map<List<SurveyViewModel>>(surveys);
            return Json(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Questions(int id)
        {
            var survey = await _surveyRepository.GetByIdWithQuestionsAsync(id);
            if (survey == null) return NotFound();

            var viewModel = new SurveyQuestionsViewModel
            {
                SurveyId = survey.Id,
                SurveyTitle = survey.Title,
                Questions = _mapper.Map<List<QuestionViewModel>>(survey.Questions)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddQuestion([FromBody] QuestionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { success = false, error = "Geçersiz model" });
                }

                // 1. Önce soruyu ekle
                var question = new SurveyQuestion
                {
                    SurveyId = model.SurveyId,
                    QuestionText = model.QuestionText ?? model.NewQuestionText
                };

                var questionId = await _surveyRepository.AddQuestionAsync(question);

                // 2. Sonra seçenekleri ekle
                var optionTexts = model.Options?.Select(o => o.OptionText).ToList() ?? 
                                 model.NewOptions ?? 
                                 new List<string>();

                await _surveyRepository.AddOptionsAsync(questionId, optionTexts);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            await _surveyRepository.DeleteQuestionAsync(id);
            return Json(new { success = true });
        }

        [HttpGet("Take/{id}")]
        public async Task<IActionResult> Take(int id)
        {
            var survey = await _surveyRepository.GetByIdWithQuestionsAsync(id);
            if (survey == null) return NotFound();

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
    }
} 