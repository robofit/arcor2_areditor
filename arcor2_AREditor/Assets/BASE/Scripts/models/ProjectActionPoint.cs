using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// ProjectActionPoint(id:str, pose:arcor2.data.Pose, actions:List[arcor2.data.Action]&#x3D;&amp;lt;factory&amp;gt;)
  /// </summary>
  [DataContract]
  public class ProjectActionPoint {
    /// <summary>
    /// Gets or Sets Actions
    /// </summary>
    /// <value>Gets or Sets Actions</value>
    [DataMember(Name="actions", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "actions")]
    public List<Action> Actions { get; set; }

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
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ProjectActionPoint {\n");
      sb.Append("  Actions: ").Append(Actions).Append("\n");
      sb.Append("  Id: ").Append(Id).Append("\n");
      sb.Append("  Pose: ").Append(Pose).Append("\n");
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
