using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.Services;
using SpectrumEngine.Emu;
using Splat;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace SpectrumEngine.Client.Avalonia.ViewModels.Shell.ToolBars
{
    public class ExecutionToolBarViewModel : ViewModelBase, IToolBar
    {
        private readonly IEmulatorService emulatorService;

        public ExecutionToolBarViewModel(IEmulatorService? emulatorService = null)
        {
            this.emulatorService = emulatorService ?? Locator.Current.GetRequiredService<IEmulatorService>();
            this.emulatorService.IsMachineControllerStateChange.Subscribe(value => MachineControllerState = value);

            InitializeCommands();

            if (this.emulatorService.MachineController != null)
            {
                this.MachineControllerState = this.emulatorService.MachineController.State;
            }
        }

        public ReactiveCommand<Unit, Unit>? StartCmd { get; private set; }
        public ReactiveCommand<Unit, Unit>? PauseCmd { get; private set; }
        public ReactiveCommand<Unit, Unit>? StopCmd { get; private set; }
        public ReactiveCommand<Unit, Unit>? ResetCmd { get; private set; }
        public ReactiveCommand<Unit, Unit>? DebugCmd { get; private set; }
        public ReactiveCommand<Unit, Unit>? DebugStepIntoCmd { get; private set; }
        public ReactiveCommand<Unit, Unit>? DebugStepOverCmd { get; private set; }
        public ReactiveCommand<Unit, Unit>? DebugStepOutCmd { get; private set; }

        [Reactive]
        public MachineControllerState MachineControllerState { get; set; }

        private void InitializeCommands()
        {
            StartCmd = ReactiveCommand.Create(() => this.emulatorService.MachineController?.Start(),
                this.WhenAnyValue(x => x.MachineControllerState).Select(item => item == MachineControllerState.Stopped || item == MachineControllerState.Paused));

            PauseCmd = ReactiveCommand.CreateFromTask(() => this.emulatorService.MachineController?.Pause() ?? Task.CompletedTask,
                this.WhenAnyValue(x => x.MachineControllerState).Select(item => item == MachineControllerState.Running));

            StopCmd = ReactiveCommand.CreateFromTask(() => this.emulatorService.MachineController?.Stop() ?? Task.CompletedTask,
                this.WhenAnyValue(x => x.MachineControllerState).Select(item => item == MachineControllerState.Running));

            ResetCmd = ReactiveCommand.CreateFromTask(() => this.emulatorService.MachineController?.Restart() ?? Task.CompletedTask);

            DebugCmd = ReactiveCommand.Create(() => this.emulatorService.MachineController?.StartDebug(),
                this.WhenAnyValue(x => x.MachineControllerState).Select(item => item == MachineControllerState.Stopped));

            DebugStepIntoCmd = ReactiveCommand.Create(() => this.emulatorService.MachineController?.StepInto(),
                this.WhenAnyValue(x => x.MachineControllerState).Select(item => item == MachineControllerState.Paused));

            DebugStepOverCmd = ReactiveCommand.Create(() => this.emulatorService.MachineController?.StepOver(),
                this.WhenAnyValue(x => x.MachineControllerState).Select(item => item == MachineControllerState.Paused));

            DebugStepOutCmd = ReactiveCommand.Create(() => this.emulatorService.MachineController?.StepOut(),
                this.WhenAnyValue(x => x.MachineControllerState).Select(item => item == MachineControllerState.Paused));
        }
    }
}
