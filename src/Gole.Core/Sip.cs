using System.Runtime.Serialization;

namespace Gole.Core;

[DataContract]
public sealed class Sip
{
    [DataMember]
    public long Utc
    {
        get; set;
    }
    [DataMember]
    public int Ml
    {
        get; set;
    }
}
