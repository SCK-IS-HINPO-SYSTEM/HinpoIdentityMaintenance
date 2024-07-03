using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HinpoIdentityMaintenance.Pages {
    [AllowAnonymous]
    public class DummyModel : PageModel {
        readonly IConfiguration _appSettings;

        public DummyModel(IConfiguration appSettings) {
            _appSettings = appSettings;
        }
        public IActionResult OnGet() {
#pragma warning disable CS8600
            string hostName = _appSettings.GetSection("NetWorkInfo:HostName").Value;
#pragma warning restore CS8600
            var returnURL = "%2FHinpoIdentityMaintenance";
            return Redirect("https://" + hostName + "/HinpoMenu/Identity/Account/Login?ReturnUrl=" + returnURL);
        }
    }
}