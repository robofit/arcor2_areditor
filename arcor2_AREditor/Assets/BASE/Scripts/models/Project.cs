using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

    /// <summary>
    /// Project(id:str, scene_id:str, objects:List[arcor2.data.ProjectObject]&#x3D;&lt;factory&gt;, desc:str&#x3D;&lt;factory&gt;)
    /// </summary>
    [DataContract]
    public class Project {
        /// <summary>
        /// Gets or Sets Desc
        /// </summary>
        [DataMember(Name = "desc", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "desc")]
        public string Desc {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "id")]
        public string Id {
            get; set;
        }

        /// <summary>
        /// Gets or Sets Objects
        /// </summary>
        [DataMember(Name = "objects", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "objects")]
        public List<ProjectObject> Objects {
            get; set;
        }

        /// <summary>
        /// Gets or Sets SceneId
        /// </summary>
        [DataMember(Name = "scene_id", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "scene_id")]
        public string SceneId {
            get; set;
        }


        /// <summary>
        /// Get the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Project {\n");
            sb.Append("  Desc: ").Append(Desc).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Objects: ").Append(Objects).Append("\n");
            sb.Append("  SceneId: ").Append(SceneId).Append("\n");
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
