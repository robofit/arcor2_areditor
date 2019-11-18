using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ARServer.Models {

    [DataContract]
    public class EventSceneChanged {

        public EventSceneChanged() {
            Event = "SceneChanged";
        }

        [DataMember(Name = "event", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "event")]
        public string Event {
            get; set;
        }

        [DataMember(Name = "data", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "data")]
        public IO.Swagger.Model.Scene Scene {
            get; set;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Event {\n");
            sb.Append("  Name: ").Append(Event).Append("\n");
            sb.Append("  Scene: ").Append(Scene).Append("\n");
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

