using HinpoIdentityMaintenance.Common;
using Microsoft.AspNetCore.Authorization;
using static CommonLibrary.Enum;

namespace HinpoIdentityMaintenance {
    public class HinpoIdentityMaintenanceAuthorizationMiddleware {
        private readonly RequestDelegate _next;
        private readonly IAuthorizationService _authorizationServices;
        public HinpoIdentityMaintenanceAuthorizationMiddleware(
            RequestDelegate next,
            IAuthorizationService authorizationServices) {
            this._next = next;
            this._authorizationServices = authorizationServices;
        }

        /// <summary>
        /// 認可ポリシーのチェックを行いエラーの場合、専用のエラーページへ遷移する
        /// 画面遷移を行うたびにInvokeAsyncが呼ばれる
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context) {
            await _next(context);
        }
    }
}
