using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// Model of sphere.
  /// </summary>
  [DataContract]
  public class Sphere {
    /// <summary>
    /// Gets or sets sphere radius.
    /// </summary>
    /// <value>Gets or sets sphere radius.</value>
    [DataMember(Name="radius", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "radius")]
    public float? Radius { get; set; }

    /// <summary>
    /// Gets or sets pose.
    /// </summary>
    /// <value>Gets or sets pose.</value>
    [DataMember(Name="pose", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "pose")]
    public Pose Pose { get; set; }

    /// <summary>
    /// Gets or sets id.
    /// </summary>
    /// <value>Gets or sets id.</value>
    [DataMember(Name="id", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class Sphere {\n");
      sb.Append("  Radius: ").Append(Radius).Append("\n");
      sb.Append("  Pose: ").Append(Pose).Append("\n");
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
