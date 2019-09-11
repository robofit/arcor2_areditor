using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;

namespace ARServer.Models {

    [DataContract]
    public class ResponseGetObjectTypesModel {

       
        [DataMember(Name = "type", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "type")]
        public IO.Swagger.Model.MetaModel3d.TypeEnum Type {
            get; set;
        }

        [DataMember(Name = "box", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "box", NullValueHandling = NullValueHandling.Ignore)]
        public IO.Swagger.Model.Box Box {
            get; set;
        }

        [DataMember(Name = "sphere", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "sphere", NullValueHandling = NullValueHandling.Ignore)]
        public IO.Swagger.Model.Sphere Sphere {
            get; set;
        }

        [DataMember(Name = "cylinder", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "cylinder", NullValueHandling = NullValueHandling.Ignore)]
        public IO.Swagger.Model.Cylinder Cylinder {
            get; set;
        }

        [DataMember(Name = "mesh", EmitDefaultValue = false)]
        [JsonProperty(PropertyName = "mesh", NullValueHandling = NullValueHandling.Ignore)]
        public IO.Swagger.Model.Mesh Mesh {
            get; set;
        }

        

        public override string ToString() {
            var sb = new StringBuilder();
            sb.Append("class Event {\n");
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

