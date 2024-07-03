using HinpoIdentityBusinessLayer;
using HinpoIdentityModels;
using HinpoIdentityMaintenance.Data;
using HinpoMasterBusinessLayer;
using HinpoMasterModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HinpoIdentityMaintenance {
    public class HinpoIdentityMaintenanceMiddleware : IMiddleware {
        private readonly Claim _claim;
        private readonly ILogger<HinpoIdentityMaintenanceMiddleware> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHinpoMasterServiceReadOnly _masterSvcRead;
        private readonly IHinpoIdentityService _identity;
        private readonly IHttpContextAccessor _httpContextAccessor;
        JsonSerializerOptions _jsonOptions = new JsonSerializerOptions() {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
#pragma warning disable CS8600,CS8601,CS8602,CS8618
        public HinpoIdentityMaintenanceMiddleware(
            ILogger<HinpoIdentityMaintenanceMiddleware> logger,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> SignInManager,
            IHttpContextAccessor httpContextAccessor,
            IHinpoMasterServiceReadOnly masterSvcRead,
            IHinpoIdentityService identity) {
            this._logger = logger;
            this._claim = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier);
            this._userManager = userManager;
            this._signInManager = SignInManager;
            this._masterSvcRead = masterSvcRead;
            this._identity = identity;
            this._httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
            //HinpoIdentityMaintenanceのセッションからユーザー情報を抜き出す
            string? jsonString = context.Session.GetString("LoginUserInfo");
            //コンテキストのUserからユーザーIDを抜き出す
            string UserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
            //デフォルトは既にセッション保存済みなので不要
            bool saveFlg = false;
            //UserIdが空ということはHinpoMenuでログインしてるので基本的にはない                
            if (string.IsNullOrEmpty(jsonString)) {
                if (!string.IsNullOrEmpty(UserId)) {
                    //DBからユーザー情報を取得
                    jsonString = (string)await GetLoginUserInfo(UserId);
                    saveFlg = true;
                }
            } else {
                CommonLibrary.LoginUserInfo UserLoginInfo = CommonLibrary.CommonFunctions.JsonParseUtil<CommonLibrary.LoginUserInfo>(jsonString);
                //セッションに入っているユーザIDと異なる場合
                if (!string.IsNullOrEmpty(UserId) &&
                    UserId != UserLoginInfo.UserGid) {
                    //DBから新しいユーザー情報を取得
                    jsonString = (string)await GetLoginUserInfo(UserId);
                    saveFlg = true;
                }
            }
            //セッションに保存が必要な場合
            if (saveFlg == true && !string.IsNullOrEmpty(jsonString)) {
                context.Session.SetString("LoginUserInfo", jsonString.ToString());
            }

            //言語設定を行うように追加
            CommonLibrary.CommonFunctions.UserCultureChange(_httpContextAccessor);

            await next(context);
        }

        /// <summary>
        /// ログインユーザー情報の取得
        /// </summary>
        /// <returns></returns>
        private async Task<string> GetLoginUserInfo(string UserId) {

            AspNetUser user = await _identity.GetAspNetUsers(_claim.Value);

            #region ログインユーザー情報の保存
            String LastName = user.LastName;
            String FirstName = user.FirstName;


            bool langFlag = false;// false:ja true:en
            AspNetUserClaim uc = user.AspNetUserClaims.FirstOrDefault(x => x.ClaimType.Equals("Lang", StringComparison.OrdinalIgnoreCase));
            string lang = uc.ClaimValue;
            if (!string.IsNullOrEmpty(lang)) {
                langFlag = lang.Equals("En", StringComparison.OrdinalIgnoreCase) ? true : false;
            }

            //テーブルからの取得時に必要な言語情報をセットする
            _masterSvcRead.SetLang(langFlag);

            int siteid = -1;
            uc = user.AspNetUserClaims.FirstOrDefault(x => x.ClaimType.Equals("SiteId", StringComparison.OrdinalIgnoreCase));
            Int32.TryParse(uc.ClaimValue.ToString(), out siteid);
            M02Site m02 = _masterSvcRead.GetM02Site(siteid).Result;

            int busyoid = -1;
            uc = user.AspNetUserClaims.FirstOrDefault(x => x.ClaimType.Equals("BusyoId", StringComparison.OrdinalIgnoreCase));
            Int32.TryParse(uc.ClaimValue.ToString(), out busyoid);
            M04Busyo m04 = _masterSvcRead.GetM04Busyo(busyoid).Result;

            List<string> Roles = new List<string>();
            foreach (var item in user.AspNetUserRoles) {
                // item.AspNetRoles のnullチェック
                if (!(item.AspNetRoles == null)) {
                    Roles.Add(item.AspNetRoles.Id);
                }
            }

            CommonLibrary.LoginUserInfo loginUserInfo = new CommonLibrary.LoginUserInfo() {
                UserGid = user.Id,
                LastName = LastName,
                FirstName = FirstName,
                SiteId = siteid,
                SiteName = m02?.SiteName,
                BusyoId = busyoid,
                BusyoName = m04?.BusyoName,
                BusyoNameAbb = m04?.BusyoNameAbb,
                Roles = Roles,
                Lang = langFlag,    //false:ja true:en
                BusyoKind = m04.BusyoKind,
            };
            string jsonString = "";
            jsonString = JsonSerializer.Serialize<CommonLibrary.LoginUserInfo>(loginUserInfo, _jsonOptions);
            return (jsonString);
            #endregion ログインユーザー情報の保存 
        }
    }
#pragma warning restore CS8600,CS8601,CS8602,CS8618
    /// <summary>
    // Extension method used to add the middleware to the HTTP request pipeline.
    /// </summary>
    public static class CustomMiddlewareExtensions {
        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder) {
            return builder.UseMiddleware<HinpoIdentityMaintenanceMiddleware>();
        }
    }


}
