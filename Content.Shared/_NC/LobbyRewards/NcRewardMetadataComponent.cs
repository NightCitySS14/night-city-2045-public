using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._NC.LobbyRewards;

/// <summary>
///     Holds metadata for items in the Lobby Rewards system.
///     Configured via YAML prototypes.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NcRewardMetadataComponent : Component
{
    /// <summary>
    ///     Item rarity: COMMON, UNCOMMON, RARE, ICONIC, LEGENDARY.
    /// </summary>
    [DataField]
    public string Rarity = "COMMON";

    /// <summary>
    ///     Optional custom color override for UI highlights.
    /// </summary>
    [DataField]
    public Color? CustomColor;

    /// <summary>
    ///     Price in Night Coins. If set, item will appear in the market.
    /// </summary>
    [DataField]
    public int? MarketPrice;

    /// <summary>
    ///     Cost in "Start Points" for the round loadout budget system.
    ///     If zero or unset, the item costs nothing to deploy.
    /// </summary>
    [DataField]
    public int PointsCost;

    /// <summary>
    ///     Category for UI filtering in the vault/market.
    ///     Examples: "Armor", "Weapons", "Medicine", "Upgrades", "Skins".
    /// </summary>
    [DataField]
    public string Category = "Misc";

    /// <summary>
    ///     Sub-category for more granular filtering in the market tabs.
    ///     Examples: "Items", "Upgrades", "Skins".
    /// </summary>
    [DataField]
    public string MarketTab = "Items";
}
