using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ARServer.Models {

    [DataContract]
    public class DoubleParameterExtra : BaseParameterExtra
    {

        public DoubleParameterExtra() {
            
        }

        [DataMember(Name = "minimum", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "minimum")]
        public double Minimum {
            get; set;
        }

        [DataMember(Name = "maximum", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "maximum")]
        public double Maximum {
            get; set;
        }

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class DoubleParameterExtra {\n");
            sb.Append("  Minimum: ").Append(Minimum).Append("\n");
            sb.Append("  Maximum: ").Append(Maximum).Append("\n");
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

