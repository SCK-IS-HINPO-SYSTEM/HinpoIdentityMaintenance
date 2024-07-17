using CommonWebResources.WebParts;
using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance.Common;
using HinpoIdentityMaintenance.Data;
using HinpoIdentityMaintenance.Models.Model;
using HinpoIdentityModels;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;
using WorkFlowModels;

namespace HinpoIdentityMaintenance.Pages.AspNetUserSearch {
    public class IndexModel : PageModel {
        private readonly IConfiguration _appSettings;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _hinpoIdentityService;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;
        private SrchCondModel _SrchCondModel;
        [BindProperty]
        public AspNetUserSearchPageModel PgModel { get; set; }
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
            PgModel = new AspNetUserSearchPageModel();
            _SrchCondModel = new SrchCondModel();
        }

        public void OnGet(string srchcond) {
            PgModel.SrchCond = srchcond;
            if (PgModel.SrchCond?.Length > 0) {
                _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            }
            PgModel.SiteId = _SrchCondModel.Srch_SiteId  ;
            PgModel.UserId = _SrchCondModel.Srch_Uid ;
            PgModel.UserName = _SrchCondModel.Srch_Name;

            SetMasterData();

            if (_SrchCondModel.Srch_SelectedUid.Length > 0) {   // ユーザーが一意に決まっている場合
                AspNetUser usr = _hinpoIdentityService.GetAspNetUsers(_SrchCondModel.Srch_SelectedUid).Result;
                if (usr != null) {
                    PgModel.SelectedUserId = usr.Id;
                    PgModel.AspNetUsers.Add(usr);                   
                }
            }
        }
        public IActionResult OnPost() {
            SetMasterData();
            if (PgModel.SrchCond?.Length > 0) {
                _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            }
            _SrchCondModel.Srch_SelectedUid = PgModel.SelectedUserId ?? "";
            _SrchCondModel.Srch_SiteId = PgModel.SiteId;
            _SrchCondModel.Srch_Uid = PgModel.UserId ?? "";
            _SrchCondModel.Srch_Name = PgModel.UserName ?? "";
 

            PgModel.SrchCond = JsonSerializer.Serialize<SrchCondModel>(_SrchCondModel, Consts._jsonOptions);
 
            
            switch (PgModel.Instruction){
                case "srch":
                    PgModel.AspNetUsers = _hinpoIdentityService.GetAspNetUsersAmbiguous(PgModel.SiteId, PgModel.UserId ?? "", PgModel.UserName ?? "").Result;
                    if (PgModel.SrchCond?.Length > 0) {
                        _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
                    }
                    break;
                case "back":
                    return RedirectPermanent("/hinpomenu/index");                       // ユーザー選択画面表示
                case "del":
                    bool rslt =_hinpoIdentityService.DeleteUser(PgModel.SelectedUserId).Result;
                    PgModel.AspNetUsers = _hinpoIdentityService.GetAspNetUsersAmbiguous(PgModel.SiteId, PgModel.UserId ?? "", PgModel.UserName ?? "").Result;
                    break;
                case "conf":
                    return RedirectToPage("/AspNetUserClaimsMnt/Index", new { srchcond = PgModel.SrchCond });
                case "auth":
                    return RedirectToPage("/AspNetUserRolesMnt/Index", new { srchcond = PgModel.SrchCond });
            }
            return Page(); ;
        }

        private void SetMasterData() {
            int mySiteId = 0;
            if (PgModel.SiteId <= 0) {
                mySiteId = int.Parse(User.FindFirstValue("SiteId") ?? "-1");
                PgModel.SiteId = mySiteId;
            } else {
                mySiteId = PgModel.SiteId;
            }
            PgModel.m02Sites = DropDownList.GetM02SitesSelectList(_masterSvcRead, mySiteId.ToString());
        }
    }
}
