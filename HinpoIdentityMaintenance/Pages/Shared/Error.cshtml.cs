using System;

namespace HinpoIdentityMaintenance.Pages {
    public class ErrorModel {
#pragma warning disable CS8618
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string ExceptionMessage { get; set; }
#pragma warning restore CS8618

    }
}
