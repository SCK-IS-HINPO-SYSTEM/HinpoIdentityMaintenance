using CommonWebResources.WebParts;
using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance.Data;
using HinpoIdentityMaintenance.Models.Model.AspNetUserSearchModel;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace HinpoIdentityMaintenance.Pages.AspNetUserSearch {
    public class IndexModel : PageModel {
        private readonly IConfiguration _appSettings;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _hinpoIdentityService;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;

        [BindProperty]
        public AspNetUserSearchPageModel PageModel { get; set; }

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
            PageModel = new AspNetUserSearchPageModel();
        }

        public void OnGet() {
            SetMasterDate();
        }
        public void OnPost() {
            SetMasterDate();
            List<HinpoIdentityModels.AspNetUser> AspNetUsers = _hinpoIdentityService.GetAspNetUsersAmbiguous(PageModel.SiteId, PageModel.UserId ?? "", PageModel.UserName ?? "").Result;
        }

        private void SetMasterDate() {
            int mySiteId = 0;
            if (PageModel.SiteId <= 0) {
                mySiteId = int.Parse(User.FindFirstValue("SiteId") ?? "-1");
                PageModel.SiteId = mySiteId;
            } else {
                mySiteId = PageModel.SiteId;
            }
            PageModel.m02Sites = DropDownList.GetM02SitesSelectList(_masterSvcRead, mySiteId.ToString());
        }
    }
}
