using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ARServer.Models {

    [DataContract]
    public class RequestNewObjectType : RequestModel {

        public RequestNewObjectType() {
            Request = "newObjectType";
        }

        [DataMember(Name = "args", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "args")]
        public RequestNewObjectTypeArgs Args {
            get; set;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Event {\n");
            sb.Append("  Request: ").Append(Request).Append("\n");
            sb.Append("  Args: ").Append(Args).Append("\n");
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

