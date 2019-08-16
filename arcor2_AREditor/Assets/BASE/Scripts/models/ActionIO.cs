using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

    /// <summary>
    /// ActionIO(default:str)
    /// </summary>
    [DataContract]
    public class ActionIO {
        /// <summary>
        /// Gets or Sets Default
        /// </summary>
        [DataMember(Name = "default", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "default")]
        public string Default {
            get; set;
        }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class ActionIO {\n");
            sb.Append("  Default: ").Append(Default).Append("\n");
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
