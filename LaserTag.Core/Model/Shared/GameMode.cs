using System.Text.Json.Serialization;

namespace LaserTag.Core.Model.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GameMode
{
    TeamDeathmatch,
    CaptureTheFlag
}
