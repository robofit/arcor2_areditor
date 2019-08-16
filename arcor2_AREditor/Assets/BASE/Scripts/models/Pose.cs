using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

    /// <summary>
    /// Pose(position:arcor2.data.Position, orientation:arcor2.data.Orientation)
    /// </summary>
    [DataContract]
    public class Pose {
        /// <summary>
        /// Gets or Sets Orientation
        /// </summary>
        [DataMember(Name = "orientation", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "orientation")]
        public Orientation Orientation {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Position
        /// </summary>
        [DataMember(Name = "position", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "position")]
        public Position Position {
            get; set;
        }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Pose {\n");
            sb.Append("  Orientation: ").Append(Orientation).Append("\n");
            sb.Append("  Position: ").Append(Position).Append("\n");
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
