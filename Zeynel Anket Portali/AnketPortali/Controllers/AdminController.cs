using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using AnketPortali.Repositories;
using AnketPortali.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AnketPortali.Models;
using Microsoft.AspNetCore.Authorization;
using AnketPortali.ViewModels.Survey;
using AnketPortali.ViewModels.Question;
using AnketPortali.ViewModels.Take;

namespace AnketPortali.Controllers
{
    [Authorize(Roles="Admin")]
    public class AdminController : Controller
    {
       
        private readonly INotyfService _notyfService;
        private readonly IMapper _mapper;
        private readonly UserRepository _userRepository;

     

        public AdminController(INotyfService notyfService, IMapper mapper, UserRepository userRepository)
        {
            _notyfService = notyfService;
            _mapper = mapper;
            _userRepository = userRepository;
           
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> UserList()
        {
            var users = await _userRepository.GetAllAsync();
          
            return View(users);
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _userRepository.DeleteAsync(id);
            if (result)
            {
                _notyfService.Success("Kullanıcı başarıyla silindi");
            }
            else
            {
                _notyfService.Error("Kullanıcı silinirken bir hata oluştu");
            }
            return RedirectToAction(nameof(UserList));
        }

       
    }
}
