using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// SceneObject(id:str, type:str, pose:arcor2.data.Pose)
  /// </summary>
  [DataContract]
  public class SceneObject {
    /// <summary>
    /// Gets or Sets Id
    /// </summary>
    /// <value>Gets or Sets Id</value>
    [DataMember(Name="id", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or Sets Pose
    /// </summary>
    /// <value>Gets or Sets Pose</value>
    [DataMember(Name="pose", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "pose")]
    public Pose Pose { get; set; }

    /// <summary>
    /// Gets or Sets Type
    /// </summary>
    /// <value>Gets or Sets Type</value>
    [DataMember(Name="type", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "type")]
    public string Type { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class SceneObject {\n");
      sb.Append("  Id: ").Append(Id).Append("\n");
      sb.Append("  Pose: ").Append(Pose).Append("\n");
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
