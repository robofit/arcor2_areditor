using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

    /// <summary>
    /// ProjectActionPoint(id:str, pose:arcor2.data.Pose, actions:List[arcor2.data.Action]&#x3D;&lt;factory&gt;)
    /// </summary>
    [DataContract]
    public class ProjectActionPoint : ActionPoint {
        /// <summary>
        /// Gets or Sets Actions
        /// </summary>
        [DataMember(Name = "actions", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "actions")]
        public List<Action> Actions {
            get; set;
        }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class ProjectActionPoint {\n");
            sb.Append("  Actions: ").Append(Actions).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Get the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public new string ToJson() {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

    }
}
