using Newtonsoft.Json;
using System.Collections.Generic;

namespace auth.ViewModels
{
    [JsonObject(MemberSerialization.OptOut)]
    public class JsonResponse
    {
        #region Constructor
        public JsonResponse(object data, string errorMessage = null)
        {
            Data = data;
            Error = errorMessage;
        }
        #endregion

        #region Properties
        public object Data { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }
        #endregion
    }
}

