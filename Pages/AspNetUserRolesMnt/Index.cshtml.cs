using DocumentFormat.OpenXml.Spreadsheet;
using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance.Common;
using HinpoIdentityMaintenance.Data;
using HinpoIdentityMaintenance.Models.Model;
using HinpoIdentityModels;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace HinpoIdentityMaintenance.Pages.AspNetUserRolesMnt {
    public class IndexModel : PageModel {
        private readonly IConfiguration _appSettings;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _hinpoIdentityService;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;

        [BindProperty]
        public AspNetUserRolesMntPageModel PgModel { get; set; }

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
            PgModel = new AspNetUserRolesMntPageModel();
        }

        public void OnGet(string srchcond) {
            PgModel = new AspNetUserRolesMntPageModel();
            PgModel.SrchCond = srchcond;
            SrchCondModel _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            PgModel.FilterRole = _SrchCondModel.Srch_RolesMnt_RoleName;            
            SetMasterData();
        }

        public IActionResult OnPost() {
            List<string> addRoles = new List<string>();
            List<string> delRoles = new List<string>();
            if (PgModel == null ) {
                SetMasterData();
                return Page();
            }
            
            // 権限フィルタを検索条件に保存
            SrchCondModel _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            _SrchCondModel.Srch_RolesMnt_RoleName = PgModel.FilterRole;
            _SrchCondModel.Srch_ProcessId = PgModel.ProcessId;
            PgModel.SrchCond = JsonSerializer.Serialize(_SrchCondModel, Consts._jsonOptions);

            switch (PgModel.Instruction) {
                case "back":
                    return RedirectToPage("/AspNetUserSearch/Index", new { srchcond = PgModel.SrchCond });
                case "srch":
                    break;
                case "upd":
                    for (int row = 0; row < PgModel.AllAspNetRoles.Count; row++) {
                        if (PgModel.AllAspNetRoles[row].IsAddChecked) {
                            addRoles.Add(PgModel.AllAspNetRoles[row].Id);
                        }
                        if (PgModel.AllAspNetRoles[row].IsDelChecked) {
                            delRoles.Add(PgModel.AllAspNetRoles[row].Id);
                        }
                    }
                    // AspNetRolesの更新。エラー時は中でthrowしているのでupdStsはチェックしなくてよい
                    bool updSts = _hinpoIdentityService.InsertOrUpdateAspNetUserRoles(_SrchCondModel.Srch_SelectedUid, addRoles, delRoles).Result;
                    SetMasterData();
                    return RedirectToPage("/AspNetUserSearch/Index", new { srchcond = PgModel.SrchCond });
            }
            SetMasterData();
            return Page();
        }
        private void SetMasterData() {
            SrchCondModel _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            PgModel.MyAspNetUser = _hinpoIdentityService.GetAspNetUsers(_SrchCondModel.Srch_SelectedUid).Result;
            PgModel.FullName = PgModel.MyAspNetUser.LastName + " " + PgModel.MyAspNetUser.FirstName;
            PgModel.SetMaster(_masterSvcRead, _hinpoIdentityService);
        }
    }
}
