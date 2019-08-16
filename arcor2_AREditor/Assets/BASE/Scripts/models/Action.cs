using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

    /// <summary>
    /// Action(id:str, type:str, parameters:List[arcor2.data.ActionParameter]&#x3D;&lt;factory&gt;, inputs:List[arcor2.data.ActionIO]&#x3D;&lt;factory&gt;, outputs:List[arcor2.data.ActionIO]&#x3D;&lt;factory&gt;)
    /// </summary>
    [DataContract]
    public class Action {
        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "id")]
        public string Id {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Inputs
        /// </summary>
        [DataMember(Name = "inputs", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "inputs")]
        public List<ActionIO> Inputs {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Outputs
        /// </summary>
        [DataMember(Name = "outputs", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "outputs")]
        public List<ActionIO> Outputs {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Parameters
        /// </summary>
        [DataMember(Name = "parameters", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "parameters")]
        public List<ActionParameter> Parameters {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "type")]
        public string Type {
            get; set;
        }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Action {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Inputs: ").Append(Inputs).Append("\n");
            sb.Append("  Outputs: ").Append(Outputs).Append("\n");
            sb.Append("  Parameters: ").Append(Parameters).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
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
