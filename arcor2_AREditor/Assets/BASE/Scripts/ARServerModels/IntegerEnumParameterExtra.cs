using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ARServer.Models {

    [DataContract]
    public class IntegerEnumParameterExtra : BaseParameterExtra
    {

        public IntegerEnumParameterExtra() {
            
        }

        [DataMember(Name = "allowed_values", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "allowed_values")]
        public List<int> AllowedValues {
            get; set;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class IntegerEnumParameterExtra {\n");
            sb.Append("  AllowedValues: ").Append(AllowedValues).Append("\n");
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

