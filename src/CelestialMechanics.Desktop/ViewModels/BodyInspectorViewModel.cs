using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CelestialMechanics.Desktop.Services;
using CelestialMechanics.Physics.Types;

namespace CelestialMechanics.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Body Inspector panel.
/// Displays and allows editing of the currently selected body's properties.
/// </summary>
public sealed partial class BodyInspectorViewModel : ObservableObject
{
    private readonly SimulationService _simService;
    private readonly SceneService _sceneService;
    private int? _currentBodyId;

    [ObservableProperty]
    private bool _hasSelection;

    // ── Identity ───────────────────────────────────────────────────
    [ObservableProperty]
    private string _bodyName = "";

    [ObservableProperty]
    private BodyType _bodyType;

    [ObservableProperty]
    private int _bodyId;

    // ── Transform ──────────────────────────────────────────────────
    [ObservableProperty]
    private double _positionX;

    [ObservableProperty]
    private double _positionY;

    [ObservableProperty]
    private double _positionZ;

    [ObservableProperty]
    private double _velocityX;

    [ObservableProperty]
    private double _velocityY;

    [ObservableProperty]
    private double _velocityZ;

    // ── Physical ───────────────────────────────────────────────────
    [ObservableProperty]
    private double _mass;

    [ObservableProperty]
    private double _radius;

    [ObservableProperty]
    private double _density;

    // ── Simulation ─────────────────────────────────────────────────
    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private bool _isCollidable;

    /// <summary>All BodyType values for the type ComboBox.</summary>
    public IReadOnlyList<BodyType> BodyTypes { get; } =
        Enum.GetValues<BodyType>().ToList().AsReadOnly();

    public BodyInspectorViewModel(SimulationService simService, SceneService sceneService)
    {
        _simService = simService;
        _sceneService = sceneService;
    }

    /// <summary>
    /// Loads a body's properties into the inspector from the simulation engine.
    /// </summary>
    public void LoadBody(Guid nodeId)
    {
        var bodyId = _sceneService.GetBodyIdForNode(nodeId);
        if (bodyId == null)
        {
            ClearSelection();
            return;
        }

        _currentBodyId = bodyId.Value;

        _simService.WithEngineLock(engine =>
        {
            if (engine.Bodies == null) return;
            foreach (var body in engine.Bodies)
            {
                if (body.Id != bodyId.Value) continue;

                BodyId = body.Id;
                BodyName = $"{body.Type} {body.Id}";
                BodyType = body.Type;
                PositionX = body.Position.X;
                PositionY = body.Position.Y;
                PositionZ = body.Position.Z;
                VelocityX = body.Velocity.X;
                VelocityY = body.Velocity.Y;
                VelocityZ = body.Velocity.Z;
                Mass = body.Mass;
                Radius = body.Radius;
                Density = body.Density;
                IsActive = body.IsActive;
                IsCollidable = body.IsCollidable;
                break;
            }
        });

        HasSelection = true;
    }

    /// <summary>
    /// Clears the inspector when no body is selected.
    /// </summary>
    public void ClearSelection()
    {
        _currentBodyId = null;
        HasSelection = false;
    }

    [RelayCommand]
    private void ApplyChanges()
    {
        if (_currentBodyId == null) return;
        int id = _currentBodyId.Value;

        _simService.WithEngineLock(engine =>
        {
            if (engine.Bodies == null) return;
            for (int i = 0; i < engine.Bodies.Length; i++)
            {
                if (engine.Bodies[i].Id != id) continue;

                engine.Bodies[i].Mass = Mass;
                engine.Bodies[i].Radius = Radius;
                engine.Bodies[i].Position = new Math.Vec3d(PositionX, PositionY, PositionZ);
                engine.Bodies[i].Velocity = new Math.Vec3d(VelocityX, VelocityY, VelocityZ);
                engine.Bodies[i].Type = BodyType;
                engine.Bodies[i].IsActive = IsActive;
                engine.Bodies[i].IsCollidable = IsCollidable;
                break;
            }
        });
    }
}
