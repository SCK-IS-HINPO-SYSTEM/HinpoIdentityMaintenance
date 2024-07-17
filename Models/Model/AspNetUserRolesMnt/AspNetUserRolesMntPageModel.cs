using CommonWebResources.WebParts;
using HinpoIdentityBusinessLayer;
using HinpoIdentityMaintenance.Common;
using HinpoIdentityModels;
using HinpoMasterBusinessLayer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using static CommonLibrary.Enum;

namespace HinpoIdentityMaintenance.Models.Model {
    /// <summary>
    /// 検査実績モデルクラス
    /// </summary>
    public class AspNetUserRolesMntPageModel {
        public List<SelectListItem> m02Sites { get; set; }
        public List<SelectListItem> m04Busyos { get; set; }
        public List<SelectListItem> processIds { get; set; }

        public int SiteId { get; set; }
        public int BusyoId { get; set; }
        public string Lang { get; set; } = default!;
        public string Instruction { get; set; } = default!;
        public string SelectedAspNetRolesId { get; set; } = default!;
        public string AspNetRolesId { get; set; } = default!;
        public string SrchCond { get; set; } = default!;        
        public string FullName { get; set; } = default!;
        public string FilterRole { get; set; } = default!;
        public short ProcessId { get; set; } = 0;
        
        public AspNetUser MyAspNetUser { get; set; } = default!;
        public List<AspNetUserRoles> MyAspNetUserRoles { get; set; } = new List<AspNetUserRoles>();
        public List<AspNetRolesExt> AllAspNetRoles { get; set; } = new List<AspNetRolesExt>();

        public void SetMaster(IHinpoMasterServiceReadOnly masterSvcRead, IHinpoIdentityService _hinpoIdentityService) {
            SrchCondModel _SrchCondModel = JsonSerializer.Deserialize<SrchCondModel>(SrchCond, Consts._jsonOptions) ?? new SrchCondModel();
            MyAspNetUserRoles = _hinpoIdentityService.GetAspNetUserRoles(_SrchCondModel.Srch_SelectedUid).Result;
            for (int i = MyAspNetUserRoles.Count - 1; i >= 0; i--) {
                if (MyAspNetUserRoles[i].AspNetRoles == null) { // 念の為マスタ不正対応
                    MyAspNetUserRoles.RemoveAt(i);
                }
            }
            if (_SrchCondModel.Srch_RolesMnt_RoleName == null) {
                _SrchCondModel.Srch_RolesMnt_RoleName = "";
            }
            List<AspNetRoles> ars = _hinpoIdentityService.GetAspNetRoles().Result;
            AllAspNetRoles = new List<AspNetRolesExt>();
            AllAspNetRoles.Clear();
            foreach (AspNetRoles ar in ars) {

                _SrchCondModel.Srch_RolesMnt_RoleName = _SrchCondModel.Srch_RolesMnt_RoleName.Trim().ToUpper();
                bool skipFlg = false;
                if (_SrchCondModel.Srch_RolesMnt_RoleName.Length > 0) {
                    if(!ar.Id.ToUpper().Contains(_SrchCondModel.Srch_RolesMnt_RoleName) && !ar.RoleNameJp.ToUpper().Contains(_SrchCondModel.Srch_RolesMnt_RoleName) && !ar.Name.ToUpper().Contains(_SrchCondModel.Srch_RolesMnt_RoleName) && !ar.NormalizedName.Contains(_SrchCondModel.Srch_RolesMnt_RoleName)) {
                        skipFlg = true;
                    }
                }
                
                if (_SrchCondModel.Srch_ProcessId > 0) {
                    if (ar.ProcessId != _SrchCondModel.Srch_ProcessId) {
                        skipFlg = true;
                    }
                }

                if (skipFlg) continue;

                AspNetRolesExt tmp = new AspNetRolesExt(ar);
                if (MyAspNetUserRoles.Any(x => x.AspNetRoles.Id == tmp.Id)) {
                    tmp.IsInRole = true;
                }
                AllAspNetRoles.Add(tmp);
            }
        }

        public AspNetUserRolesMntPageModel() {
            m02Sites = new List<SelectListItem>();
            m04Busyos = new List<SelectListItem>();
            AllAspNetRoles = new List<AspNetRolesExt>();
            processIds = new List<SelectListItem>();
            foreach (eProcessId prcs in Enum.GetValues(typeof(eProcessId))) {
                SelectListItem item = new SelectListItem();
                item.Value = ((int)prcs).ToString();
                item.Text = prcs.ToString();
                processIds.Add(item);
            }
            Instruction = "";
            Lang = "";
        }
    }
    public class AspNetRolesExt : AspNetRoles {
        public bool IsInRole { get; set; } = false;
        public bool IsAddChecked { get; set; } = false;
        public bool IsDelChecked { get; set; } = false;

        public AspNetRolesExt() {
        }

        public AspNetRolesExt(AspNetRoles ar) {
            base.Id = ar.Id;
            base.RoleId = ar.RoleId;
            base.RoleNameJp = ar.RoleNameJp;
            base.ProcessId = ar.ProcessId;
            base.GroupId = ar.GroupId;
            base.RoleId = ar.RoleId;
            base.Name = ar.Name;
            base.NormalizedName = ar.NormalizedName;
        }
    }
}
