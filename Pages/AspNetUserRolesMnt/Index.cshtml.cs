using DocumentFormat.OpenXml.Spreadsheet;
using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance.Data;
using HinpoIdentityMaintenance.Models.Model;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

namespace HinpoIdentityMaintenance.Pages.AspNetUserRolesMnt {
    public class IndexModel : PageModel {
        private readonly IConfiguration _appSettings;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _hinpoIdentityService;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;

        [BindProperty]
        public AspNetUserRolesMntPageModel PageModel { get; set; }

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
            PageModel = new AspNetUserRolesMntPageModel();
        }

        public void OnGet(string userid) {
            PageModel.UserId = userid;
            SetMasterData();
        }

        public IActionResult OnPost() {
            List<string> modRoles = new List<string>();
            if (PageModel == null ) {
                SetMasterData();
                return Page();
            }
            switch (PageModel.Instruction) {
                case "back":
                    return RedirectToPage("/AspNetUserSearch/Index", new { userid = PageModel.UserId });
                case "upd":
                    for (int row = 0; row < PageModel.AllAspNetRoles.Count; row++) {
                        if (PageModel.AllAspNetRoles[row].IsChecked) {
                            modRoles.Add(PageModel.AllAspNetRoles[row].Id);
                        }
                    }
                    // AspNetRolesの更新。エラー時は中でthrowしているのでupdStsはチェックしなくてよい
                    bool updSts = _hinpoIdentityService.InsertOrUpdateAspNetUserRoles(PageModel.UserId, modRoles).Result;
                    SetMasterData();
                    return RedirectToPage("/AspNetUserSearch/Index", new { userid = PageModel.UserId });
            }
            SetMasterData();
            return Page(); ;
        }
        private void SetMasterData() {
            PageModel.MyAspNetUser = _hinpoIdentityService.GetAspNetUsers(PageModel.UserId).Result;
            PageModel.FullName = PageModel.MyAspNetUser.LastName + " " + PageModel.MyAspNetUser.FirstName;
            PageModel.SetMaster(_masterSvcRead, _hinpoIdentityService);
        }
    }
}
