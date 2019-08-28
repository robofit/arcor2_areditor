using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// Model of project sources with script and resources.
  /// </summary>
  [DataContract]
  public class ProjectSources {
    /// <summary>
    /// Gets or Sets Id
    /// </summary>
    /// <value>Gets or Sets Id</value>
    [DataMember(Name="id", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or Sets Resources
    /// </summary>
    /// <value>Gets or Sets Resources</value>
    [DataMember(Name="resources", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "resources")]
    public string Resources { get; set; }

    /// <summary>
    /// Gets or Sets Script
    /// </summary>
    /// <value>Gets or Sets Script</value>
    [DataMember(Name="script", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "script")]
    public string Script { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ProjectSources {\n");
      sb.Append("  Id: ").Append(Id).Append("\n");
      sb.Append("  Resources: ").Append(Resources).Append("\n");
      sb.Append("  Script: ").Append(Script).Append("\n");
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
