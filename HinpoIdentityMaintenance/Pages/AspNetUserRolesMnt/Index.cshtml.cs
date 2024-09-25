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
using WorkFlowBusinessLayer;
using System.Text.Json;

namespace HinpoIdentityMaintenance.Pages.AspNetUserRolesMnt {
    /// <summary>
    /// 権限御画面のコードビハインド
    /// </summary>
    public class IndexModel : PageModel {
        private readonly IConfiguration _appSettings;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _hinpoIdentityService;
        private readonly IWorkFlowService _workflowService;
        private readonly IHttpContextAccessor _accessor;
        private readonly UserManager<ApplicationUser> _userMgr;
        private readonly SignInManager<ApplicationUser> _signInMgr;

        [BindProperty]
        public AspNetUserRolesMntPageModel PgModel { get; set; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="appSettings"></param>
        /// <param name="userMgr"></param>
        /// <param name="signInMgr"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="masterSvcRead"></param>
        /// <param name="hinpoIdentityService"></param>
        public IndexModel(
        IConfiguration appSettings,
            UserManager<ApplicationUser> userMgr,
            SignInManager<ApplicationUser> signInMgr,
            IHttpContextAccessor httpContextAccessor,
            IHinpoMasterServiceReadOnly masterSvcRead,
            IHinpoIdentityService hinpoIdentityService,
            IWorkFlowService workflowService) {
            _appSettings = appSettings;
            _userMgr = userMgr;
            _signInMgr = signInMgr;
            _accessor = httpContextAccessor;
            _masterSvcRead = masterSvcRead;
            _hinpoIdentityService = hinpoIdentityService;
            _workflowService = workflowService;
            PgModel = new AspNetUserRolesMntPageModel();
        }

        /// <summary>
        /// 初期表示
        /// </summary>
        /// <param name="srchcond"></param>
        public void OnGet(string srchcond) {
            PgModel = new AspNetUserRolesMntPageModel();
            PgModel.SrchCond = srchcond;
            SrchCondModel _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            PgModel.FilterRole = _SrchCondModel.Srch_RolesMnt_RoleName;            
            SetMasterData();
        }

        /// <summary>
        /// 権限編集処理
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPost() {
            List<string> addRoles = new List<string>();
            List<string> delRoles = new List<string>();
            string? loginUserName = "";
            loginUserName = User?.Identity?.Name;
            if (loginUserName == null || loginUserName.Length == 0) {
                throw (new Exception("Not Logged In"));
            }
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
                    if (updSts) {
                        List<AspNetRoles> aspNetRolesTmp = _hinpoIdentityService.GetAspNetRoles().Result;
                        List<AspNetRoles> aspNetRoles = aspNetRolesTmp.Where(x=>x.GroupId > 0 && x.RoleId > 0).ToList();

                        List<Tuple<short, short, short>> addRoleInf = new List<Tuple<short, short, short>>();
                        List<Tuple<short, short, short>> delRoleInf = new List<Tuple<short, short, short>>();
                        
                        //権限付与するRoles情報取得
                        foreach(string role in addRoles) {
                            AspNetRoles? mAspNetRoles = aspNetRoles.FirstOrDefault(x => x.Id.Equals(role) && x.GroupId > 0 && x.RoleId > 0);
                            if(mAspNetRoles != null) {
                                addRoleInf.Add(new Tuple<short, short, short>(mAspNetRoles.ProcessId, mAspNetRoles.GroupId, mAspNetRoles.RoleId));
                            }
                        }

                        //権限剥奪するRoles情報取得
                        foreach (string role in delRoles) {
                            AspNetRoles? mAspNetRoles = aspNetRoles.FirstOrDefault(x => x.Id.Equals(role) && x.GroupId > 0 && x.RoleId > 0);
                            if (mAspNetRoles != null) {
                                delRoleInf.Add(new Tuple<short, short, short>(mAspNetRoles.ProcessId, mAspNetRoles.GroupId, mAspNetRoles.RoleId));
                            }
                        }
                        if (addRoleInf.Count > 0 || delRoleInf.Count > 0) {
                            _workflowService.SetGid(loginUserName);
                            updSts = _workflowService.InsertOrDeleteM04userGrp(_SrchCondModel.Srch_SelectedUid, addRoleInf, delRoleInf).Result;
                        }
                    }
                    SetMasterData();
                    return RedirectToPage("/AspNetUserSearch/Index", new { srchcond = PgModel.SrchCond });
            }
            SetMasterData();
            return Page();
        }

        /// <summary>
        /// 表示用マスタデータをセット
        /// </summary>
        private void SetMasterData() {
            SrchCondModel _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(PgModel.SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            PgModel.MyAspNetUser = _hinpoIdentityService.GetAspNetUsers(_SrchCondModel.Srch_SelectedUid).Result;
            PgModel.FullName = PgModel.MyAspNetUser.LastName + " " + PgModel.MyAspNetUser.FirstName;
            PgModel.SetMaster(_masterSvcRead, _hinpoIdentityService);
        }
    }
}
