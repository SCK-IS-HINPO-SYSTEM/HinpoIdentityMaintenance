using CommonWebResources.WebParts;
using HinpoIdentityMaintenance.Common;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HinpoIdentityMaintenance.Models.Model.AspNetUserSearchModel {
    /// <summary>
    /// 検査実績モデルクラス
    /// </summary>
    public class AspNetUserSearchPageModel {
  
        public List<SelectListItem> m02Sites { get; set; }




        public AspNetUserSearchPageModel(IHinpoMasterServiceReadOnly hinpoMasterSvcRead) {
            m02Sites = DropDownList.GetM02SitesSelectList(hinpoMasterSvcRead, "K");
            //_commonLogic.SetListDefaultSelected(InspectionResultsFlgList, InspecPageIo.InspectionResultsFlg.ToString() ?? "");
        }
    }
}
