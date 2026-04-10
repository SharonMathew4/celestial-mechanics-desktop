using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using CelestialMechanics.Desktop.Services.Messaging;

namespace CelestialMechanics.Desktop.ViewModels.Shell;

public sealed partial class StatusBarViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string _fpsText = "FPS: --";

    [ObservableProperty]
    private string _simTimeText = "t=0.000";

    public StatusBarViewModel()
    {
        IsActive = true;
        Messenger.Register<StateUpdateMessage>(this, (_, message) =>
        {
            FpsText = $"Physics: {message.PhysicsStepMs:F2} ms";
            SimTimeText = $"t={message.SimulationTime:F4}";
        });
    }
}
