using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

    /// <summary>
    /// ProjectSources(id:str, resources:str, script:str)
    /// </summary>
    [DataContract]
    public class ProjectSources {
        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "id")]
        public string Id {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Resources
        /// </summary>
        [DataMember(Name = "resources", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "resources")]
        public string Resources {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Script
        /// </summary>
        [DataMember(Name = "script", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "script")]
        public string Script {
            get; set;
        }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class ProjectSources {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Resources: ").Append(Resources).Append("\n");
            sb.Append("  Script: ").Append(Script).Append("\n");
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
