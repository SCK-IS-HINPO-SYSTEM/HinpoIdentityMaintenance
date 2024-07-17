using DocumentFormat.OpenXml.Spreadsheet;
using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance.Common;
using HinpoIdentityMaintenance.Data;
using HinpoIdentityMaintenance.Models.Model;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace HinpoIdentityMaintenance.Pages.AspNetUserClaimsMnt {
    public class IndexModel : PageModel {
        private readonly IConfiguration _appSettings;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _hinpoIdentityService;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;
 
        [BindProperty]
        public AspNetUserClaimsMntPageModel PgModel { get; set; }

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
            PgModel = new AspNetUserClaimsMntPageModel();
        }

        public void OnGet(string srchcond) {
            PgModel.SrchCond = srchcond;
            SrchCondModel _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            SetMasterData();
            ViewData["Srch_SelectedUid"] = _SrchCondModel.Srch_SelectedUid;
        }

        public IActionResult OnPost() {
            bool updSts = false;
            SrchCondModel _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            switch (PgModel.Instruction) {
                case "back":
                    return RedirectToPage("/AspNetUserSearch/Index", new { srchcond = PgModel.SrchCond });
                case "upd":
                    updSts = _hinpoIdentityService.InsertOrUpdateAspNetUserClaims(_SrchCondModel.Srch_Uid, "SiteId", PgModel.SiteId.ToString() ).Result;
                    if (updSts == false) {
                        throw new Exception("SiteId Update Failed");
                    }
                    updSts = _hinpoIdentityService.InsertOrUpdateAspNetUserClaims(_SrchCondModel.Srch_Uid, "BusyoId", PgModel.BusyoId.ToString()).Result;
                    if (updSts == false) {
                        throw new Exception("BusyoId Update Failed");
                    }
                    updSts = _hinpoIdentityService.InsertOrUpdateAspNetUserClaims(_SrchCondModel.Srch_Uid, "Lang", PgModel.Lang).Result;
                    if (updSts == false) {
                        throw new Exception("Language Update Failed");
                    }
                    SetMasterData();

                    return RedirectToPage("/AspNetUserSearch/Index", new { userid = _SrchCondModel.Srch_Uid });
            }
            SetMasterData();
            return Page(); ;
        }
        private void SetMasterData() {
            if (PgModel.SrchCond?.Length > 0) {
                SrchCondModel _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
                PgModel.MyAspNetUser = _hinpoIdentityService.GetAspNetUsers(_SrchCondModel.Srch_SelectedUid).Result;
            }
            PgModel.SetMaster(_masterSvcRead, _hinpoIdentityService);
        }
    }
}
