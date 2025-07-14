using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.RemoteControl;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class RemoteControlComponent : Component
{
    [DataField]
    public EntityWhitelist? EntityWhitelist;

    [DataField]
    public bool Enabled = true;


    [DataField, AutoNetworkedField]
    public EntityUid? Controller;

    [DataField, AutoNetworkedField]
    public EntityUid? Controllable;

    public bool IsActive => Controller != null && Controllable != null;
}


