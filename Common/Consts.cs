using HinpoIdentityBusinessLayer;
using HinpoIdentityModels;
using HinpoIdentityMaintenance.Models;
using Org.BouncyCastle.Cms;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HinpoIdentityMaintenance.Common {
    public static class Consts {   
        public static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions() {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All),
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };
    }
}
