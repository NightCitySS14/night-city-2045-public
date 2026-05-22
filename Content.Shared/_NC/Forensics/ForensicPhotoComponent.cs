using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Map;

namespace Content.Shared._NC.Forensics;

/// <summary>
/// Компонент для "фотоснимка" места преступления.
/// Хранит данные для отображения на карте при активации предмета.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ForensicPhotoComponent : Component
{
    [DataField, AutoNetworkedField]
    public string VictimName = "Unknown";

    [DataField, AutoNetworkedField]
    public string LocationName = "Unknown";

    [DataField, AutoNetworkedField]
    public NetCoordinates Coordinates;

    [DataField, AutoNetworkedField]
    public TimeSpan Timestamp;
}
