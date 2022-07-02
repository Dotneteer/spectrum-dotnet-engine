# The `ZxSpectrum48Machine` Implementation

The `ZxSpectrum48Machine`, which class derives from `Z80MachineBase`, implements the behavior of a ZX Spectrum 48K computer. The implementation delegates the emulation of hardware components to a set of devices:
- `KeyboardDevice`: Implements the keyboard's behavior; allows setting and querying the state of individual keys.
- `ScreenDevice`: Implements the screen rendering of the ULA chip. This device works simultaneously with the Z80 CPU and displays the subsequent pixels as the CPU executes the instructions. *This device creates the ARGB (32-bit) bitmap of the screen to display, but it does not undertake the task of showing it on the UI.*
- `BeeperDevice`: Represents the one-bit beeper of ZX Spectrum 48, creating the samples that produce the emulated sound on the host machine. *Though this device creates the sound samples, it does not send them to a physical sound device.*
- `TapeDevice`: Emulates the flow of binary signal stream (EAR bit) coming from a tape (to allow loading data) and implements the tape signal (MIC bit) flow when saving data to the tape.
- `ZxSpectrum48FloatingBusDevice`: Emulates the behavior of the ZX Spectrum 48K's floating bus.

The `ZxSpectrum48Machine` class overrides and extends the behavior of its ancestor class, `Z80MachineBase`, at these points:

- Implementing the reset behavior
- Implementing the behavior of the memory
  - Separating the ROM and RAM partitions
  - Delaying memory access according to the contention with the ULA
- Implementing the behavior of output ports
  - Handling reading and writing from even ports
  - Delaying I/O access according to the contention with the ULA
  - Dispatching the responsibility of handling input and output bits to specific devices
  - Emulating some hardware-related behavior, such as reading the Bit 6 value according to the last writes of Bit 3/Bit 4(due to a capacitor in the hardware)
- Setting the screen width and height values
- Providing access to the screen's bitmap as rendered during the execution of a machine frame
- Handling the emulation of queued keystrokes
- Overriding the `OnInitNewFrame` method to allow the screen and beeper devices to respond to the start of a new machine frame.
- Overriding the `ShouldRaiseInterrupt` method to provide the INT signal behavior of the ZX Spectrum 48K
- Overriding the `AfterInstructionExecuted` method to let the beeper device render a sound sample and let the tape device detect passive/LOAD/SAVE mode changes
- Overriding the `OnTactIncremented` method to emulate that the ULA renders the display pixels simultaneously with the Z80 CPU

## Reset and Hard Reset

_TBD_ 

## Memory Handling

_TBD_

## I/O Handling

_TBD_

## Screen Rendering

_TBD_

## Keystroke Emulation

_TBD_

## Machine Frame Execution Overrides

