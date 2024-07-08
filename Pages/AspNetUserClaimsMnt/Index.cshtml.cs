using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance.Data;
using HinpoIdentityMaintenance.Models.Model;
using HinpoIdentityModels;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HinpoIdentityMaintenance.Pages.AspNetUserClaimsMnt {
    public class IndexModel : PageModel {
        private readonly IConfiguration _appSettings;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _hinpoIdentityService;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;

        [BindProperty]
        public AspNetUserClaimsMntPageModel PageModel { get; set; }

        public IndexModel(
            IConfiguration appSettings,
            UserManager<ApplicationUser> userMgr,
            SignInManager<ApplicationUser> signInMgr,
            IHttpContextAccessor httpContextAccessor,
            IHinpoMasterServiceReadOnly masterSvcRead,
            IHinpoIdentityService hinpoIdentityService) {
            _appSettings = appSettings;
            _userMgr = userMgr;
            _signInMgr = signInMgr;
            _accessor = httpContextAccessor;
            _masterSvcRead = masterSvcRead;
            _hinpoIdentityService = hinpoIdentityService;
            PageModel = new AspNetUserClaimsMntPageModel();
        }

        public void OnGet(string userid) {
            PageModel.MyAspNetUser = _hinpoIdentityService.GetAspNetUsers(userid).Result;
            PageModel.SetMaster(_masterSvcRead, _hinpoIdentityService);
        }
    }
}
