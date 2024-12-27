using System.Collections.Generic;
using System.Threading.Tasks;
using AnketPortali.Models;
using AnketPortali.Repositories;
using Microsoft.EntityFrameworkCore;
using AutoMapper;


namespace SurveyApp.Repositories
{
    public class SurveyRepository : GenericRepository<Survey>
    {
        private readonly AppDbContext _context;

        public SurveyRepository(AppDbContext context) : base(context, context.Surveys)
        {
            _context = context;
        }

        public async Task<Survey> GetByIdWithCategoryAsync(int id)
        {
            return await _context.Surveys
                .Include(s => s.Category)
                .Include(s => s.CreatedBy)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Survey> GetByIdWithQuestionsAsync(int id)
        {
            return await _context.Surveys
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Survey>> GetAllWithCategoryAsync()
        {
            return await _context.Surveys
                .Include(s => s.Category)
                .Include(s => s.CreatedBy)
                .ToListAsync();
        }

        public async Task SaveAnswerAsync(SurveyResponse response)
        {
            await _context.SurveyResponses.AddAsync(response);
            await _context.SaveChangesAsync();
        }

        public async Task<Survey> GetByIdWithAllDetailsAsync(int id)
        {
            return await _context.Surveys
                .Include(s => s.Category)
                .Include(s => s.CreatedBy)
                .Include(s => s.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<int> AddQuestionAsync(SurveyQuestion question)
        {
            _context.SurveyQuestions.Add(question);
            await _context.SaveChangesAsync();
            return question.Id;
        }

        public async Task AddOptionsAsync(int questionId, List<string> optionTexts)
        {
            var options = optionTexts.Select(text => new SurveyOption
            {
                QuestionId = questionId,
                OptionText = text
            }).ToList();

            _context.SurveyOptions.AddRange(options);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteQuestionAsync(int questionId)
        {
            var responses = await _context.SurveyResponses
                .Where(r => r.QuestionId == questionId)
                .ToListAsync();
            
            if (responses.Any())
            {
                _context.SurveyResponses.RemoveRange(responses);
            }

            var options = await _context.SurveyOptions
                .Where(o => o.QuestionId == questionId)
                .ToListAsync();
            
            if (options.Any())
            {
                _context.SurveyOptions.RemoveRange(options);
            }

            var question = await _context.SurveyQuestions.FindAsync(questionId);
            if (question != null)
            {
                _context.SurveyQuestions.Remove(question);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuestionAsync(SurveyQuestion question)
        {
            var existingOptions = await _context.SurveyOptions
                .Where(o => o.QuestionId == question.Id)
                .ToListAsync();
            
            _context.SurveyOptions.RemoveRange(existingOptions);

            _context.SurveyQuestions.Update(question);

            if (question.Options != null && question.Options.Any())
            {
                foreach (var option in question.Options)
                {
                    option.QuestionId = question.Id;
                }
                await _context.SurveyOptions.AddRangeAsync(question.Options);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<SurveyQuestion> GetQuestionWithOptionsAsync(int questionId)
        {
            return await _context.SurveyQuestions
                .Include(q => q.Options)
                .FirstOrDefaultAsync(q => q.Id == questionId);
        }

        public async Task<List<SurveyOption>> GetOptionsForQuestion(int questionId)
        {
            return await _context.SurveyOptions
                .Where(o => o.QuestionId == questionId)
                .ToListAsync();
        }

        public async Task<bool> HasUserParticipatedInSurvey(int surveyId, string userId)
        {
            return await _context.SurveyResponses
                .AnyAsync(r => r.SurveyId == surveyId && r.UserId == userId);
        }
    }
} 