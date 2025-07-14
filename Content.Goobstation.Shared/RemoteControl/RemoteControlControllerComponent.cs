using Content.Shared.Whitelist;

namespace Content.Goobstation.Shared.RemoteControl;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class RemoteControlControllerComponent : Component
{
    [DataField]
    public EntityWhitelist? PilotWhitelist;

    [DataField]
    public bool Enabled = true;


    [DataField, AutoNetworkedField]
    public EntityUid? Controller;

    [DataField, AutoNetworkedField]
    public EntityUid? Controllable;

    public bool IsActive => Controller != null && Controllable != null;
}
