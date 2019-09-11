using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ARServer.Models {

    [DataContract]
    public class RequestFocusObjectArgs {

       
        [DataMember(Name = "object_id", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "object_id")]
        public string ObjectId {
            get; set;
        }
                       
        [DataMember(Name = "point_idx", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "point_idx")]
        public int PointIdx {
            get; set;
        }
                       

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Event {\n");
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

