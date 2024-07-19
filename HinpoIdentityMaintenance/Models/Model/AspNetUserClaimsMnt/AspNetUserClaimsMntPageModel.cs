using CommonWebResources.WebParts;
using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance.Common;
using HinpoIdentityModels;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using static CommonLibrary.Enum;

namespace HinpoIdentityMaintenance.Models.Model {
    /// <summary>
    /// 検査実績モデルクラス
    /// </summary>
    public class AspNetUserClaimsMntPageModel {
        public List<SelectListItem> m02Sites { get; set; }
        public List<SelectListItem> m04Busyos { get; set; }
        public int SiteId { get; set; }
        public int BusyoId { get; set; }
        public string Lang { get; set; } = default!;
        public string Instruction { get; set; } = default!;
        public string SrchCond { get; set; } = default!;
        public AspNetUser MyAspNetUser = default!;
        public List<AspNetUserClaim> MyAspNetUserClaims = new List<AspNetUserClaim>();

        public void SetMaster(IHinpoMasterServiceReadOnly masterSvcRead, IHinpoIdentityService _hinpoIdentityService) {
            int mySiteId;
            string tmp = MyAspNetUser.AspNetUserClaims.FirstOrDefault(x => x.ClaimType.Equals("SiteId"))?.ClaimValue ?? "";
            Int32.TryParse(tmp, out mySiteId);
            this.SiteId = mySiteId;
            int myBusyoId;
            tmp = MyAspNetUser.AspNetUserClaims.FirstOrDefault(x => x.ClaimType.Equals("BusyoId"))?.ClaimValue ?? "";
            Int32.TryParse(tmp, out myBusyoId);
            this.BusyoId = myBusyoId;

            m02Sites = DropDownList.GetM02SitesSelectList(masterSvcRead, mySiteId.ToString());
            m04Busyos = DropDownList.GetM04BusyosSelectList(masterSvcRead, myBusyoId.ToString());
            Lang = MyAspNetUser.AspNetUserClaims.FirstOrDefault(x => x.ClaimType.Equals("Lang"))?.ClaimValue ?? "";
        }

        public AspNetUserClaimsMntPageModel() {
            m02Sites = new List<SelectListItem>();
            m04Busyos = new List<SelectListItem>();
            Instruction = "";
            Lang = "";
        }
    }
}
