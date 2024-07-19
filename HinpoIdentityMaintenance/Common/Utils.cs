using CommonLibrary;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HinpoIdentityMaintenance.Common {
    public static class Utils {
        public static string GetSession(IHttpContextAccessor accessor, string sName) {
#pragma warning disable CS8602, CS8603 
            return accessor.HttpContext.Session.GetString(sName);
#pragma warning restore CS8602, CS8603 
        }
    }
}