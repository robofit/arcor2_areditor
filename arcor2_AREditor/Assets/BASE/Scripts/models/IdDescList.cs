using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

    /// <summary>
    /// IdDescList(items:List[arcor2.data.IdDesc]&#x3D;&lt;factory&gt;)
    /// </summary>
    [DataContract]
    public class IdDescList {
        /// <summary>
        /// Gets or Sets Items
        /// </summary>
        [DataMember(Name = "items", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "items")]
        public List<IdDesc> Items {
            get; set;
        }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class IdDescList {\n");
            sb.Append("  Items: ").Append(Items).Append("\n");
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
