using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ARServer.Models {

    [DataContract]
    public class RequestFocusObjectStartArgs {

       
        [DataMember(Name = "object_id", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "object_id")]
        public string ObjectId {
            get; set;
        }

        [DataMember(Name = "robot_id", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "robot_id")]
        public string RobotId {
            get; set;
        }

        [DataMember(Name = "end_effector", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "end_effector")]
        public string EndEffector {
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

