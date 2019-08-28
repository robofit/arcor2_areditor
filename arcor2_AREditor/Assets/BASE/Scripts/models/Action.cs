using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace IO.Swagger.Model {

  /// <summary>
  /// Describes the action that can be executed by Pick Master.
  /// </summary>
  [DataContract]
  public class Action {
    /// <summary>
    /// Gets or Sets Id
    /// </summary>
    /// <value>Gets or Sets Id</value>
    [DataMember(Name="id", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    /// <summary>
    /// Gets or Sets Inputs
    /// </summary>
    /// <value>Gets or Sets Inputs</value>
    [DataMember(Name="inputs", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "inputs")]
    public List<ActionIO> Inputs { get; set; }

    /// <summary>
    /// Gets or Sets Outputs
    /// </summary>
    /// <value>Gets or Sets Outputs</value>
    [DataMember(Name="outputs", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "outputs")]
    public List<ActionIO> Outputs { get; set; }

    /// <summary>
    /// Gets or Sets Parameters
    /// </summary>
    /// <value>Gets or Sets Parameters</value>
    [DataMember(Name="parameters", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "parameters")]
    public List<ActionParameter> Parameters { get; set; }

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
      sb.Append("class Action {\n");
      sb.Append("  Id: ").Append(Id).Append("\n");
      sb.Append("  Inputs: ").Append(Inputs).Append("\n");
      sb.Append("  Outputs: ").Append(Outputs).Append("\n");
      sb.Append("  Parameters: ").Append(Parameters).Append("\n");
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
