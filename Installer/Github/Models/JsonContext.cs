using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Installer.Github.Models;

[JsonSerializable(typeof(List<Release>))]
public partial class ReleasesJsonContext : JsonSerializerContext
{
}
