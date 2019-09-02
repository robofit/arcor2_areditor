using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// Model of box.
  /// </summary>
  [DataContract]
  public class Box {
    /// <summary>
    /// Gets or sets the size of the box in X-axis.
    /// </summary>
    /// <value>Gets or sets the size of the box in X-axis.</value>
    [DataMember(Name="sizeX", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "sizeX")]
    public float? SizeX { get; set; }

    /// <summary>
    /// Gets or sets the size of the box in Y-axis.
    /// </summary>
    /// <value>Gets or sets the size of the box in Y-axis.</value>
    [DataMember(Name="sizeY", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "sizeY")]
    public float? SizeY { get; set; }

    /// <summary>
    /// Gets or sets the size of the box in Z-axis.
    /// </summary>
    /// <value>Gets or sets the size of the box in Z-axis.</value>
    [DataMember(Name="sizeZ", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "sizeZ")]
    public float? SizeZ { get; set; }

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
      sb.Append("class Box {\n");
      sb.Append("  SizeX: ").Append(SizeX).Append("\n");
      sb.Append("  SizeY: ").Append(SizeY).Append("\n");
      sb.Append("  SizeZ: ").Append(SizeZ).Append("\n");
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
