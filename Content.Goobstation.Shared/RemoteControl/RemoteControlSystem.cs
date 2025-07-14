namespace Content.Goobstation.Shared.RemoteControl;
using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Storage;
using Content.Shared.Whitelist;
using Robust.Shared.Containers;
using Robust.Shared.Timing;
using Content.Shared.Movement.Systems;
using Content.Shared.Whitelist;
public abstract class RemoteControlSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMoverController _mover = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    public override void Initialize()
    {
        Log.Debug("Shit Initialized!");
        base.Initialize();
        SubscribeLocalEvent<RemoteControlComponent, EntInsertedIntoContainerMessage>(OnEntInserted);
        SubscribeLocalEvent<RemoteControlComponent, EntRemovedFromContainerMessage>(OnEntRemoved);
        SubscribeLocalEvent<RemoteControlComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<RemoteControlComponent, GotUnequippedEvent>(OnUnequipped);
    }


    private void OnEntInserted(Entity<RemoteControlComponent> entity, ref EntInsertedIntoContainerMessage args)
    {
        Log.Debug("Entity Inserted!");
        // Make sure the entity was actually inserted into storage and not a different container.
        if (!TryComp(entity, out StorageComponent? storage) || args.Container != storage.Container)
            return;

        // Check potential pilot against whitelist, if one exists.
        if (_whitelistSystem.IsWhitelistFail(entity.Comp.EntityWhitelist, args.Entity))
            return;

        entity.Comp.Controller = args.Entity;


        // Attempt to setup control link, if Pilot and Wearer are both present.
        StartPiloting(entity);
    }

    private void OnEntRemoved(Entity<RemoteControlComponent> entity, ref EntRemovedFromContainerMessage args)
    {
        Log.Debug("Entity Removed!");
        // Make sure the removed entity is actually the pilot.
        if (args.Entity != entity.Comp.Controller)
            return;

        StopPiloting(entity);
        entity.Comp.Controller = null;

    }

    private void OnEquipped(Entity<RemoteControlComponent> entity, ref GotEquippedEvent args)
    {
        Log.Debug("Entity Equipped!");
        entity.Comp.Controllable = entity;
        // Attempt to setup control link, if Pilot and Wearer are both present.
        StartPiloting(entity);
    }

    private void OnUnequipped(Entity<RemoteControlComponent> entity, ref GotUnequippedEvent args)
    {
        Log.Debug("Entity unequipped!");
        StopPiloting(entity);

        entity.Comp.Controllable = null;

    }

    /// <summary>
    /// Attempts to establish movement/interaction relay connection(s) from Pilot to Wearer.
    /// If either is missing, fails and returns false.
    /// </summary>
    private bool StartPiloting(Entity<RemoteControlComponent> entity)
    {
        Log.Debug("Start Pilot!");
        if (entity.Comp.Controller == null || entity.Comp.Controllable == null)
            return false;

        var controller = entity.Comp.Controller.Value;
        var controllable = entity.Comp.Controllable.Value;

        if (entity.Comp.Enabled)
        {
            _mover.SetRelay(controller, controllable);
        }

        /*var pilotEv = new StartedPilotingClothingEvent(entity, wearerEnt);
        RaiseLocalEvent(pilotEnt, ref pilotEv);

        var wearerEv = new StartingBeingPilotedByClothing(entity, pilotEnt);
        RaiseLocalEvent(wearerEnt, ref wearerEv);*/

        return true;
    }

    /// <summary>
    /// Removes components from the Pilot and Wearer to stop the control relay.
    /// Returns false if a connection does not already exist.
    /// </summary>
    private bool StopPiloting(Entity<RemoteControlComponent> entity)
    {
        Log.Debug("Stop Pilot!");
        if (entity.Comp.Controller == null || entity.Comp.Controllable == null)
            return false;

        // Clean up components on the Pilot
        var pilotEnt = entity.Comp.Controller.Value;
        RemCompDeferred<RelayInputMoverComponent>(pilotEnt);

        // Clean up components on the Wearer
        var wearerEnt = entity.Comp.Controllable.Value;
        RemCompDeferred<MovementRelayTargetComponent>(wearerEnt);

        // Raise an event on the Pilot
        var pilotEv = new StoppedPilotingClothingEvent(entity, wearerEnt);
        RaiseLocalEvent(pilotEnt, ref pilotEv);

        // Raise an event on the Wearer
        var wearerEv = new StoppedBeingPilotedByClothing(entity, pilotEnt);
        RaiseLocalEvent(wearerEnt, ref wearerEv);

        return true;
    }
}

/// <summary>
/// Raised on the Pilot when they gain control of the Wearer.
/// </summary>
[ByRefEvent]
public record struct StartedPilotingClothingEvent(EntityUid Clothing, EntityUid Wearer);

/// <summary>
/// Raised on the Pilot when they lose control of the Wearer,
/// due to the Pilot exiting the clothing or the clothing being unequipped by the Wearer.
/// </summary>
[ByRefEvent]
public record struct StoppedPilotingClothingEvent(EntityUid Clothing, EntityUid Wearer);

/// <summary>
/// Raised on the Wearer when the Pilot gains control of them.
/// </summary>
[ByRefEvent]
public record struct StartingBeingPilotedByClothing(EntityUid Clothing, EntityUid Pilot);

/// <summary>
/// Raised on the Wearer when the Pilot loses control of them
/// due to the Pilot exiting the clothing or the clothing being unequipped by the Wearer.
/// </summary>
[ByRefEvent]
public record struct StoppedBeingPilotedByClothing(EntityUid Clothing, EntityUid Pilot);


