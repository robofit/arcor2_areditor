using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ARServer.Models {

    [DataContract]
    public class RequestModel {

        public RequestModel() {
            
        }

        [DataMember(Name = "request", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "request")]
        public string Request {
            get; set;
        }

       
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Request {\n");
            sb.Append("  Request: ").Append(Request).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Get the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public string ToJson() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }


    }
}

