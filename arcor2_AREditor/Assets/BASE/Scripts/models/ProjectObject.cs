using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// Model of project object with list of project action points.
  /// </summary>
  [DataContract]
  public class ProjectObject {
    /// <summary>
    /// Gets or Sets ActionPoints
    /// </summary>
    /// <value>Gets or Sets ActionPoints</value>
    [DataMember(Name="actionPoints", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "actionPoints")]
    public List<ProjectActionPoint> ActionPoints { get; set; }

    /// <summary>
    /// Gets or Sets Id
    /// </summary>
    /// <value>Gets or Sets Id</value>
    [DataMember(Name="id", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ProjectObject {\n");
      sb.Append("  ActionPoints: ").Append(ActionPoints).Append("\n");
      sb.Append("  Id: ").Append(Id).Append("\n");
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
