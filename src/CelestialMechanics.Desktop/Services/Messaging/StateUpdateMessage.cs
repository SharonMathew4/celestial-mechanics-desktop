using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.Services.Messaging;

public sealed record StateUpdateMessage(
    SimulationState State,
    double SimulationTime,
    double PhysicsStepMs);
