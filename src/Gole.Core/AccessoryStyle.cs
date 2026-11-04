using System.Runtime.Serialization;

namespace Gole.Core;

// Legacy contract retained only to read preferences created by versions 0.2.14–0.2.15.
[DataContract]
public sealed record AccessoryStyle
{
    [DataMember]
    public int AccessoryId
    {
        get; set;
    }
    [DataMember] public string Color { get; set; } = "";
    [DataMember]
    public int Pattern
    {
        get; set;
    }

}
