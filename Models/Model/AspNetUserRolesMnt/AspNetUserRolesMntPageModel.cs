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
    public class AspNetUserRolesMntPageModel {
        public List<SelectListItem> m02Sites { get; set; }
        public List<SelectListItem> m04Busyos { get; set; }
        public int SiteId { get; set; }
        public int BusyoId { get; set; }
        public string Lang { get; set; } = default!;
        public string Instruction { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public AspNetUser MyAspNetUser = default!;
        //public List<AspNetUserClaim> MyAspNetUserClaims = new List<AspNetUserClaim>();
        public List<AspNetUserRoles> MyAspNetUserRoles = new List<AspNetUserRoles>();
        public List<AspNetRolesExt> AllAspNetRoles = new List<AspNetRolesExt>();

        //public AspNetUserRolesMntPageModel(IHinpoMasterServiceReadOnly masterSvcRead, int mySiteId, int myBusyoId) {
        //    m02Sites = DropDownList.GetM02SitesSelectList(masterSvcRead, mySiteId.ToString());
        //    m04Busyos = DropDownList.GetM04BusyosSelectList(masterSvcRead, myBusyoId.ToString());
        //}

        public void SetMaster(IHinpoMasterServiceReadOnly masterSvcRead, IHinpoIdentityService _hinpoIdentityService) {
            //int mySiteId;
            //string tmp = MyAspNetUser.AspNetUserRoles.FirstOrDefault(x => x.ClaimType.Equals("SiteId"))?.ClaimValue ?? "";
            //Int32.TryParse(tmp, out mySiteId);
            //this.SiteId = mySiteId;
            //int myBusyoId;
            //tmp = MyAspNetUser.AspNetUserRoles.FirstOrDefault(x => x.ClaimType.Equals("BusyoId"))?.ClaimValue ?? "";
            //Int32.TryParse(tmp, out myBusyoId);
            //this.BusyoId = myBusyoId;

            //m02Sites = DropDownList.GetM02SitesSelectList(masterSvcRead, mySiteId.ToString());
            //m04Busyos = DropDownList.GetM04BusyosSelectList(masterSvcRead, myBusyoId.ToString());
            MyAspNetUserRoles = _hinpoIdentityService.GetAspNetUserRoles(UserId).Result;
            for(int i = MyAspNetUserRoles.Count -1; i >= 0; i--) {
                if (MyAspNetUserRoles[i].AspNetRoles == null) { // 念の為マスタ不正対応
                    MyAspNetUserRoles.RemoveAt(i);
                }
            }
            List<AspNetRoles> ars = _hinpoIdentityService.GetAspNetRoles().Result;
            foreach (AspNetRoles ar in ars) {
                AspNetRolesExt tmp = new AspNetRolesExt(ar);
                //for (int i = 0; i < MyAspNetUserRoles.Count; i++) {
                //    if (MyAspNetUserRoles[i].AspNetRoles?.Id == ar.Id) {
                //        tmp.IsChecked = true;
                //        break;
                //    }
                //}
                //foreach (var t in ars) {
                    if(MyAspNetUserRoles.Any(x=>x.AspNetRoles.Id == tmp.Id)){
                        tmp.IsChecked = true;
                    }
                //}

                AllAspNetRoles.Add(tmp);
            }

            //Lang = MyAspNetUser.AspNetUserRoles.FirstOrDefault(x => x.ClaimType.Equals("Lang"))?.ClaimValue ?? "";
        }

        public AspNetUserRolesMntPageModel() {
            m02Sites = new List<SelectListItem>();
            m04Busyos = new List<SelectListItem>();
            Instruction = "";
            Lang = "";
        }
    }
    public class AspNetRolesExt : AspNetRoles {
        public bool IsChecked { get; set; } = false;
        public AspNetRolesExt(AspNetRoles ar) {
            base.Id = ar.Id;
            base.RoleId = ar.RoleId;
            base.RoleNameJp= ar.RoleNameJp;
            base.ProcessId= ar.ProcessId;
            base.GroupId = ar.GroupId;
            base.RoleId = ar.RoleId;
            base.Name = ar.Name;
            base.NormalizedName = ar.NormalizedName;
        }

    }
}
