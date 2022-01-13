# Challenges of Developing a ZX Spectrum 48K Emulator

Here, I collected the most significant challenges you face when writing a ZX Spectrum 48K emulator:

1. You should implement the behavior of the Z80 CPU accurately. At any moment, you should know precisely the number of clock cycles (T-states) elapsed since the CPU has started. Each executed instruction must exactly as many T-states as described in the official (or non-official) documentation. Instructions must provide the same state changes (registers, flags, signals, etc.) as the actual hardware.

2. You cannot ignore implementing the two special (officially undocumented) Z80 flags and emulating the `MEMPTR` behavior. Otherwise, programs that push the flags to the stack and later use the popped values when executing conditional statements may follow a different control flow than a physical ZX Spectrum.

3. You must implement the screen rendering process accurately. While the ULA reads the pixel and attribute bytes to display, it compels the CPU to wait. The memory access delay depends on the current screen rendering tact (a single screen frame contains 69888 tacts).

5. The periodic maskable interrupt signal is always raised in the first tact (the one with index 0) of the screen rendering process, and its duration is 32 T-states. Event slight deviation from this signal timing may cause applications and games (especially those that use Interrupt Mode 2) to have weird behavior.