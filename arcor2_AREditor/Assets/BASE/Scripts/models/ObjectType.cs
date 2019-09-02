using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// Model of Object type contain id, source, model and description.
  /// </summary>
  [DataContract]
  public class ObjectType {
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
    /// Gets or Sets Source
    /// </summary>
    /// <value>Gets or Sets Source</value>
    [DataMember(Name="source", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "source")]
    public string Source { get; set; }

    /// <summary>
    /// Gets or Sets model info
    /// </summary>
    /// <value>Gets or Sets model info</value>
    [DataMember(Name="model", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "model")]
    public MetaModel3d Model { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class ObjectType {\n");
      sb.Append("  Desc: ").Append(Desc).Append("\n");
      sb.Append("  Id: ").Append(Id).Append("\n");
      sb.Append("  Source: ").Append(Source).Append("\n");
      sb.Append("  Model: ").Append(Model).Append("\n");
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
