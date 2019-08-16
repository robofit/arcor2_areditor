using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

    /// <summary>
    /// Orientation(x:float, y:float, z:float, w:float)
    /// </summary>
    [DataContract]
    public class Orientation : IterableIndexable {
        /// <summary>
        /// Gets or Sets W
        /// </summary>
        [DataMember(Name = "w", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "w")]
        public decimal? W {
            get; set;
        }

        /// <summary>
        /// Gets or Sets X
        /// </summary>
        [DataMember(Name = "x", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "x")]
        public decimal? X {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Y
        /// </summary>
        [DataMember(Name = "y", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "y")]
        public decimal? Y {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Z
        /// </summary>
        [DataMember(Name = "z", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "z")]
        public decimal? Z {
            get; set;
        }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Orientation {\n");
            sb.Append("  W: ").Append(W).Append("\n");
            sb.Append("  X: ").Append(X).Append("\n");
            sb.Append("  Y: ").Append(Y).Append("\n");
            sb.Append("  Z: ").Append(Z).Append("\n");
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
