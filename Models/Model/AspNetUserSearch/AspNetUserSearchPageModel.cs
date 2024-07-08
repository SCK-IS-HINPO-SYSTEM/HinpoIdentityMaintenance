using CommonWebResources.WebParts;
using HinpoIdentityMaintenance.Common;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HinpoIdentityMaintenance.Models.Model {
    /// <summary>
    /// 検査実績モデルクラス
    /// </summary>
    public class AspNetUserSearchPageModel {
        public string SelectedUserId { get; set; } = string.Empty;
        public string Instruction { get; set; } = string.Empty;        
        public List<SelectListItem> m02Sites { get; set; }
        public int SiteId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        public List<HinpoIdentityModels.AspNetUser> AspNetUsers;

        public AspNetUserSearchPageModel(IHinpoMasterServiceReadOnly masterSvcRead, int mySiteId) {
            m02Sites = DropDownList.GetM02SitesSelectList(masterSvcRead, mySiteId.ToString());
            AspNetUsers = new List<HinpoIdentityModels.AspNetUser>();
        }

        public AspNetUserSearchPageModel() {
            m02Sites = new List<SelectListItem>();
            AspNetUsers = new List<HinpoIdentityModels.AspNetUser>();
        }
    }
}
