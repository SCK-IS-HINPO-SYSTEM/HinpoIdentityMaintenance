using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance.Models.Model.AspNetUserSearchModel;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HinpoIdentityMaintenance.Pages.AspNetUserSearch {
    public class IndexModel : PageModel {
        private readonly IConfiguration _appSettings;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _hinpoIdentityService;

        [BindProperty]
        AspNetUserSearchPageModel PageModel { get; set; }

        public IndexModel(
            IConfiguration appSettings,
            IHinpoMasterServiceReadOnly masterSvcRead,
            IHinpoIdentityService hinpoIdentityService) {
            _appSettings = appSettings;
            _masterSvcRead = masterSvcRead;
            _hinpoIdentityService = hinpoIdentityService;
            PageModel = new AspNetUserSearchPageModel(_masterSvcRead);
        }

        public void OnGet() {
        }
    }
}
