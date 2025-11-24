using System.Text.Json.Serialization;
using server.dtos;

namespace server.tools
{
    [JsonDerivedType(typeof(TextContent), typeDiscriminator: "text")]
    [JsonDerivedType(typeof(ImageContent), typeDiscriminator: "image")]
    [JsonDerivedType(typeof(NetContent), typeDiscriminator: "net")]
    public class OutputContentGroup
    {
        
    }
}