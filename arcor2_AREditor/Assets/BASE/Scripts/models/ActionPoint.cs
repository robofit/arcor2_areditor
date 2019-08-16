using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

    /// <summary>
    /// ActionPoint(id:str, pose:arcor2.data.Pose)
    /// </summary>
    [DataContract]
    public class ActionPoint {
        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "id")]
        public string Id {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Pose
        /// </summary>
        [DataMember(Name = "pose", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "pose")]
        public Pose Pose {
            get; set;
        }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class ActionPoint {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Pose: ").Append(Pose).Append("\n");
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
