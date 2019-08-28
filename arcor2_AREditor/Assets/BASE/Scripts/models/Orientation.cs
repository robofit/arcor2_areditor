using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// Model of orientation.
  /// </summary>
  [DataContract]
  public class Orientation {
    /// <summary>
    /// Gets or Sets W
    /// </summary>
    /// <value>Gets or Sets W</value>
    [DataMember(Name="w", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "w")]
    public float? W { get; set; }

    /// <summary>
    /// Gets or Sets X
    /// </summary>
    /// <value>Gets or Sets X</value>
    [DataMember(Name="x", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "x")]
    public float? X { get; set; }

    /// <summary>
    /// Gets or Sets Y
    /// </summary>
    /// <value>Gets or Sets Y</value>
    [DataMember(Name="y", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "y")]
    public float? Y { get; set; }

    /// <summary>
    /// Gets or Sets Z
    /// </summary>
    /// <value>Gets or Sets Z</value>
    [DataMember(Name="z", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "z")]
    public float? Z { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class Orientation {\n");
      sb.Append("  W: ").Append(W).Append("\n");
      sb.Append("  X: ").Append(X).Append("\n");
      sb.Append("  Y: ").Append(Y).Append("\n");
      sb.Append("  Z: ").Append(Z).Append("\n");
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
