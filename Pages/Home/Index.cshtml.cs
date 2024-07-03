using HinpoIdentityBusinessLayer;
using HinpoIdentityModels;
using HinpoIdentityMaintenance.Data;
using HinpoMasterBusinessLayer;
using HinpoMasterModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HinpoIdentityMaintenance.Pages {
    //ログイン済みかの制限をかけるためだけにModelを作成
    [Authorize]
    public class IndexModel : PageModel {
        private readonly ILogger<IndexModel> _logger;

        private readonly Claim _claim;
        readonly IConfiguration _appSettings;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _identity;
        JsonSerializerOptions _jsonOptions = new JsonSerializerOptions() {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
#pragma warning disable CS8601,CS8602,CS8618
        public IndexModel(ILogger<IndexModel> logger,
             UserManager<ApplicationUser> userMgr,
             SignInManager<ApplicationUser> signInMgr,
             IHttpContextAccessor httpContextAccessor,
             IConfiguration appSettings,
             IHinpoMasterServiceReadOnly masterSvcRead,
             IHinpoIdentityService identity
            ) {
            _logger = logger;
            _claim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            _appSettings = appSettings;
            _signInManager = signInMgr;
            _masterSvcRead = masterSvcRead;
            _identity = identity;
        }
#pragma warning restore CS8601,CS8602,CS8618

        public void OnGet() {
            if (_claim == null) {
                ViewData["strDisabled"] = " disabled ";
            }
        }
    }
}