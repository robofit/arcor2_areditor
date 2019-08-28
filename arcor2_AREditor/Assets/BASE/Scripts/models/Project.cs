using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// Model of project with reference to scene and list with project objects.
  /// </summary>
  [DataContract]
  public class Project {
    /// <summary>
    /// Gets or Sets Desc
    /// </summary>
    /// <value>Gets or Sets Desc</value>
    [DataMember(Name="desc", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "desc")]
    public string Desc { get; set; }

    /// <summary>
    /// Gets or Sets Id
    /// </summary>
    /// <value>Gets or Sets Id</value>
    [DataMember(Name="id", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or Sets Objects
    /// </summary>
    /// <value>Gets or Sets Objects</value>
    [DataMember(Name="objects", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "objects")]
    public List<ProjectObject> Objects { get; set; }

    /// <summary>
    /// Gets or Sets SceneId
    /// </summary>
    /// <value>Gets or Sets SceneId</value>
    [DataMember(Name="sceneId", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "sceneId")]
    public string SceneId { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
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
