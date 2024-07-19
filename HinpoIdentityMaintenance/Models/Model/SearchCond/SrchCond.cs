using CommonWebResources.WebParts;
using HinpoIdentityMaintenance.Common;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HinpoIdentityMaintenance.Models.Model {
    /// <summary>
    /// 検索条件モデルクラス
    /// </summary>
    public class SrchCondModel {
        #region 検索画面条件
        public int Srch_SiteId { get; set; }
        public string Srch_Uid { get; set; }
        public string Srch_Name { get; set; }
        public string Srch_SelectedUid { get; set; }
        #endregion 検索画面条件

        #region Roles画面条件
        public string Srch_RolesMnt_RoleName { get; set; }
        public int Srch_ProcessId { get; set; }
        #endregion Roles画面条件

        public SrchCondModel() {
            Srch_Uid = "";
            Srch_Name = "";
            Srch_SelectedUid = "";
            Srch_RolesMnt_RoleName = "";
        }
    }
}
