using SpectrumEngine.Client.Avalonia.Extensions;
using SpectrumEngine.Client.Avalonia.Models;
using SpectrumEngine.Client.Avalonia.Providers;
using SpectrumEngine.Emu;
using Splat;
using System.Reactive.Subjects;

namespace SpectrumEngine.Client.Avalonia.Services
{
    public class EmulatorService : IEmulatorService
    {
        private Machine? machine;

        private MachineController? machineController;
        private readonly Subject<MachineController> isMachineChange;
        private readonly Subject<MachineControllerState> isMachineControllerStateChange;

        private IKeyboardProvider? keyboardProvider;
        private readonly Subject<IKeyboardProvider> iskeyboardProviderChange;

        public EmulatorService()
        {
            isMachineChange = new Subject<MachineController>();
            isMachineControllerStateChange = new Subject<MachineControllerState>();
            iskeyboardProviderChange = new Subject<IKeyboardProvider>();
        }

        public Machine? Machine
        {
            set
            {
                if (!value.HasValue || machine == value) return;
                machine = value;

                CreateMachine(machine.Value);
            }
            get => machine;
        }

        public MachineController? MachineController => machineController;

        public Subject<MachineController> IsMachineChange => isMachineChange;

        public Subject<MachineControllerState> IsMachineControllerStateChange => isMachineControllerStateChange;

        public IKeyboardProvider? KeyboardProvider => keyboardProvider;

        public Subject<IKeyboardProvider> IskeyboardProviderChange => iskeyboardProviderChange;

        private void CreateMachine(Machine machine)
        {
            machineController = CreateMachineController(machine);
            isMachineChange.OnNext(machineController);

            keyboardProvider = CreateKeyboardProvider(machine);
            iskeyboardProviderChange.OnNext(keyboardProvider);
        }

        private MachineController CreateMachineController(Machine machine)
        {
            if (machineController != null)
            {
                machineController.StateChanged -= MachineController_StateChanged;
            }

            var newMachineController = new MachineController(Locator.Current.GetRequiredService<IZ80Machine>(machine.ToString()));
            newMachineController.StateChanged += MachineController_StateChanged;
            return newMachineController;
        }

        private void MachineController_StateChanged(object? sender, (MachineControllerState OldState, MachineControllerState NewState) e)
        {
            isMachineControllerStateChange.OnNext(e.NewState);
        }

        private static IKeyboardProvider CreateKeyboardProvider(Machine keyboard) => Locator.Current.GetRequiredService<IKeyboardProvider>(keyboard.ToString());
    }
}
