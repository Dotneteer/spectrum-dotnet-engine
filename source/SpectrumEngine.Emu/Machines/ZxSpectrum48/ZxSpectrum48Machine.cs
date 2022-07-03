namespace SpectrumEngine.Emu;

/// <summary>
/// This class represents the emulator of a ZX Spectrum 48 machine.
/// </summary>
public sealed class ZxSpectrum48Machine :
    Z80MachineBase,
    IZxSpectrum48Machine
{
    #region Private members

    private const int AUDIO_SAMPLE_RATE = 48_000;

    // --- This byte array represents the 64K memory, including the 16K ROM and 48K RAM.
    private readonly byte[] _memory = new byte[0x1_0000];

    // --- This byte array stores the contention values associated with a particular machine frame tact.
    private byte[] _contentionValues = Array.Empty<byte>();

    // --- Stores the last rendered machine frame tact.
    private int _lastRenderedFrameTact;

    // --- Last value of bit 3 on port $FE
    private bool _portBit3LastValue;

    // --- Last value of bit 4 on port $FE
    private bool _portBit4LastValue;

    // --- Tacts value when last time bit 4 of $fe changed from 0 to 1
    private ulong _portBit4ChangedFrom0Tacts;

    // --- Tacts value when last time bit 4 of $fe changed from 1 to 0
    private ulong _portBit4ChangedFrom1Tacts;

    // --- The clock multiplier value used in the previous machine frame;
    private int _oldClockMultiplier;

    // --- Stores the key strokes to emulate
    private readonly Queue<EmulatedKeyStroke> _emulatedKeyStrokes =
        new Queue<EmulatedKeyStroke>();

    #endregion

    #region Initialization and Properties

    /// <summary>
    /// Initialize the machine
    /// </summary>
    public ZxSpectrum48Machine()
    {
        // --- Set up machine attributes
        BaseClockFrequency = 3_500_000;
        ClockMultiplier = 1;
        DelayedAddressBus = true;
        
        // --- Create and initialize devices
        KeyboardDevice = new KeyboardDevice(this);
        ScreenDevice = new ScreenDevice(this);
        BeeperDevice = new BeeperDevice(this);
        FloatingBusDevice = new ZxSpectrum48FloatingBusDevice(this);
        TapeDevice = new TapeDevice(this);
        Reset();

        // --- Initialize the machine's ROM (Roms/ZxSpectrum48/ZxSpectrum48.rom)
        UploadRomBytes(LoadRomFromResource(DefaultRomResource));
    }

    /// <summary>
    /// Specify the name of the default ROM's resource file within this assembly.
    /// </summary>
    protected override string DefaultRomResource => "ZxSpectrum48";

    /// <summary>
    /// Gets the ULA issue number of the ZX Spectrum model (2 or 3)
    /// </summary>
    public int UlaIssue { get; set; } = 3;

    /// <summary>
    /// Represents the keyboard device of ZX Spectrum 48K
    /// </summary>
    public IKeyboardDevice KeyboardDevice { get; }

    /// <summary>
    /// Represents the screen device of ZX Spectrum 48K
    /// </summary>
    public IScreenDevice ScreenDevice { get; }

    /// <summary>
    /// Represents the beeper device of ZX Spectrum 48K
    /// </summary>
    public IBeeperDevice BeeperDevice { get; }

    /// <summary>
    /// Represents the floating port device of ZX Spectrum 48K
    /// </summary>
    public IFloatingBusDevice FloatingBusDevice { get; }

    /// <summary>
    /// Represents the tape device of ZX Spectrum 48K
    /// </summary>
    public ITapeDevice TapeDevice { get; }

    /// <summary>
    /// Indicates if the currently selected ROM is the ZX Spectrum 48 ROM
    /// </summary>
    public bool IsSpectrum48RomSelected => true;

    /// <summary>
    /// Emulates turning on a machine (after it has been turned off).
    /// </summary>
    public override void HardReset()
    {
        base.HardReset();
        for (var i = 0x4000; i < _memory.Length; i++) _memory[i] = 0;
        Reset();
    }

    /// <summary>
    /// This method emulates resetting a machine with a hardware reset button.
    /// </summary>
    public override void Reset()
    {
        // --- Reset the CPU
        base.Reset();

        // --- Reset and setup devices
        KeyboardDevice.Reset();
        ScreenDevice.Reset();
        BeeperDevice.Reset();
        BeeperDevice.SetAudioSampleRate(AUDIO_SAMPLE_RATE);
        FloatingBusDevice.Reset();
        TapeDevice.Reset();
        
        // --- Set default property values
        SetMachineProperty(MachinePropNames.TapeMode, TapeMode.Passive);
        SetMachineProperty(MachinePropNames.RewindRequested, null);

        // --- Unknown clock multiplier in the previous frame
        _oldClockMultiplier = -1;

        // --- Prepare for running a new machine loop
        ClockMultiplier = TargetClockMultiplier;
        ExecutionContext.LastTerminationReason = null;
        _lastRenderedFrameTact = -0;

        // --- Empty the queue of emulated keystrokes
        lock (_emulatedKeyStrokes) { _emulatedKeyStrokes.Clear(); }
    }

    #endregion

    #region Memory Device

    /// <summary>
    /// Read the byte at the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <returns>The byte read from the memory</returns>
    public override byte DoReadMemory(ushort address)
        => _memory[address];

    /// <summary>
    /// This function implements the memory read delay of the CPU.
    /// </summary>
    /// <param name="address">Memory address to read</param>
    /// <remarks>
    /// Normally, it is exactly 3 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 3-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 3 T-states!
    /// </remarks>
    public override void DelayMemoryRead(ushort address)
    {
        DelayAddressBusAccess(address);
        TactPlus3();
    }

    /// <summary>
    /// Write the given byte to the specified memory address.
    /// </summary>
    /// <param name="address">16-bit memory address</param>
    /// <param name="value">Byte to write into the memory</param>
    public override void DoWriteMemory(ushort address, byte value)
    {
        if ((address & 0xc000) != 0x0000)
        {
            _memory[address] = value;
        }
    }

    /// <summary>
    /// This function implements the memory write delay of the CPU.
    /// </summary>
    /// <param name="address">Memory address to write</param>
    /// <remarks>
    /// Normally, it is exactly 3 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 3-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 3 T-states!
    /// </remarks>
    public override void DelayMemoryWrite(ushort address)
    {
        DelayAddressBusAccess(address);
        TactPlus3();
    }

    /// <summary>
    /// This method implements memory operation delays.
    /// </summary>
    /// <param name="address"></param>
    /// <remarks>
    /// Whenever the CPU accesses the 0x4000-0x7fff memory range, it contends with the ULA. We keep the contention
    /// delay values for a particular machine frame tact in _contentionValues.Independently of the memory address, 
    /// the Z80 CPU takes 3 T-states to read or write the memory contents.
    /// </remarks>
    public override void DelayAddressBusAccess(ushort address)
    {
        if ((address & 0xc000) == 0x4000)
        {
            // --- We read from contended memory
            var delay = _contentionValues[CurrentFrameTact / ClockMultiplier];
            TactPlusN(delay);
        }
    }

    /// <summary>
    /// This method allocates storage for the memory contention values.
    /// </summary>
    /// <param name="tactsInFrame">Number of tacts in a machine frame</param>
    /// <remarks>
    /// Each machine frame tact that renders a display pixel may have a contention delay. If the CPU reads or writes
    /// data or uses an I/O port in that particular frame tact, the memory operation may be delayed. When the machine's
    /// screen device is initialized, it calculates the number of tacts in a frame and calls this method to allocate
    /// storage for the contention values.
    /// </remarks>
    public void AllocateContentionValues(int tactsInFrame)
    {
        _contentionValues = new byte[tactsInFrame];
    }

    /// <summary>
    /// This method sets the contention value associated with the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <param name="value">Contention value</param>
    public void SetContentionValue(int tact, byte value)
    {
        _contentionValues[tact] = value;
    }

    /// <summary>
    /// This method gets the contention value for the specified machine frame tact.
    /// </summary>
    /// <param name="tact">Machine frame tact</param>
    /// <returns>The contention value associated with the specified tact.</returns>
    public byte GetContentionValue(int tact)
    {
        return _contentionValues[tact];
    }

    #endregion

    #region I/O port handling

    /// <summary>
    /// This function reads a byte (8-bit) from an I/O port using the provided 16-bit address.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port read operation.
    /// </remarks>
    public override byte DoReadPort(ushort address)
    {
        return (address & 0x0001) == 0 
            ? ReadPort0Xfe(address)
            : FloatingBusDevice.ReadFloatingPort();
    }

    /// <summary>
    /// This function implements the I/O port read delay of the CPU.
    /// </summary>
    /// <remarks>
    /// Normally, it is exactly 4 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 4-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 4 T-states!
    /// </remarks>
    public override void DelayPortRead(ushort address) => DelayContendedIo(address);

    /// <summary>
    /// This function writes a byte (8-bit) to the 16-bit I/O port address provided in the first argument.
    /// </summary>
    /// <remarks>
    /// When placing the CPU into an emulated environment, you must provide a concrete function that emulates the
    /// I/O port write operation.
    /// </remarks>
    public override void DoWritePort(ushort address, byte value)
    {
        if ((address & 0x0001) == 0)
        {
            WritePort0xFE(value);
        }
    }

    /// <summary>
    /// This function implements the I/O port write delay of the CPU.
    /// </summary>
    /// <remarks>
    /// Normally, it is exactly 4 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 4-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 4 T-states!
    /// </remarks>
    public override void DelayPortWrite(ushort address) => DelayContendedIo(address);

    /// <summary>
    /// Reads a byte from the ZX Spectrum generic input port.
    /// </summary>
    /// <param name="address">Port address</param>
    /// <returns>Byte value read from the generic port</returns>
    private byte ReadPort0Xfe(ushort address)
    {
        var portValue = KeyboardDevice.GetKeyLineStatus(address);

        // --- Check for LOAD mode
        if (TapeDevice.TapeMode == TapeMode.Load)
        {
            var earBit = TapeDevice.GetTapeEarBit();
            BeeperDevice.SetEarBit(earBit);
            portValue = (byte)((portValue & 0xbf) | (earBit ? 0x40 : 0));
        }
        else
        {
            // --- Handle analog EAR bit
            var bit4Sensed = _portBit4LastValue;
            if (!bit4Sensed)
            {
                // --- Changed later to 1 from 0 than to 0 from 1?
                var chargeTime = _portBit4ChangedFrom1Tacts - _portBit4ChangedFrom0Tacts;
                if (chargeTime > 0)
                {
                    // --- Yes, calculate charge time
                    chargeTime = chargeTime > 700 ? 2800 : 4 * chargeTime;

                    // --- Calculate time ellapsed since last change from 1 to 0
                    bit4Sensed = Tacts - _portBit4ChangedFrom1Tacts < chargeTime;
                }
            }

            // --- Calculate bit 6 value
            var bit6Value = _portBit3LastValue
              ? 0x40
              : bit4Sensed
                ? 0x40
                : 0x00;

            // --- Check for ULA 3
            if (UlaIssue == 3 && _portBit3LastValue && !bit4Sensed)
            {
                bit6Value = 0x00;
            }

            // --- Merge bit 6 with port value
            portValue = (byte)((portValue & 0xbf) | bit6Value);
        }
        return portValue;
    }

    /// <summary>
    /// Wites the specified data byte to the ZX Spectrum generic output port.
    /// </summary>
    /// <param name="value">Data byte to write</param>
    // ReSharper disable once InconsistentNaming
    private void WritePort0xFE(byte value)
    {
        // --- Extract bthe border color
        ScreenDevice.BorderColor = value & 0x07;

        // --- Store the last EAR bit
        var bit4 = value & 0x10;
        BeeperDevice.SetEarBit(bit4 != 0);

        // --- Set the last value of bit3
        _portBit3LastValue = (value & 0x08) != 0;

        // --- Instruct the tape device process the MIC bit
        TapeDevice.ProcessMicBit(_portBit3LastValue);

        // --- Manage bit 4 value
        if (_portBit4LastValue)
        {
            // --- Bit 4 was 1, is it now 0?
            if (bit4 == 0)
            {
                _portBit4ChangedFrom1Tacts = Tacts;
                _portBit4LastValue = false;
            }
        }
        else
        {
            // --- Bit 4 was 0, is it now 1?
            if (bit4 != 0)
            {
                _portBit4ChangedFrom0Tacts = Tacts;
                _portBit4LastValue = true;
            }
        }
    }

    /// <summary>
    /// Delays the I/O access according to address bus contention
    /// </summary>
    /// <param name="address"></param>
    private void DelayContendedIo(ushort address)
    {
        var lowbit = (address & 0x0001) != 0;

        // --- Check for contended range
        if ((address & 0xc000) == 0x4000)
        {
            if (lowbit)
            {
                // --- Low bit set, C:1, C:1, C:1, C:1
                ApplyContentionDelay();
                TactPlus1();
                ApplyContentionDelay();
                TactPlus1();
                ApplyContentionDelay();
                TactPlus1();
                ApplyContentionDelay();
                TactPlus1();
            }
            else
            {
                // --- Low bit reset, C:1, C:3
                ApplyContentionDelay();
                TactPlus1();
                ApplyContentionDelay();
                TactPlus3();
            }
        }
        else
        {
            if (lowbit)
            {
                // --- Low bit set, N:4
                TactPlus4();
            }
            else
            {
                // --- Low bit reset, C:1, C:3
                TactPlus1();
                ApplyContentionDelay();
                TactPlus3();
            }
        }

        // --- Apply I/O contention
        void ApplyContentionDelay()
        {
            var delay = GetContentionValue(CurrentFrameTact / ClockMultiplier);
            TactPlusN(delay);
        }
    }

    #endregion

    #region Display

    /// <summary>
    /// Width of the screen in native machine screen pixels
    /// </summary>
    public override int ScreenWidthInPixels => ScreenDevice.ScreenWidth;

    /// <summary>
    /// Height of the screen in native machine screen pixels
    /// </summary>
    public override int ScreenHeightInPixels => ScreenDevice.ScreenLines;

    /// <summary>
    /// Gets the buffer that stores the rendered pixels
    /// </summary>
    public override uint[] GetPixelBuffer() => ScreenDevice.GetPixelBuffer();

    #endregion
    
    #region Keyboard
    
    /// <summary>
    /// Set the status of the specified ZX Spectrum key.
    /// </summary>
    /// <param name="key">Key code</param>
    /// <param name="isDown">Indicates if the key is pressed down.</param>
    public override void SetKeyStatus(SpectrumKeyCode key, bool isDown)
    {
        KeyboardDevice.SetStatus(key, isDown);
    }

    /// <summary>
    /// Emulates queued key strokes as if those were pressed by the user
    /// </summary>
    public override void EmulateKeystroke()
    {
        // --- Exit, if no keystroke to emulate
        lock (_emulatedKeyStrokes)
        {
            if (_emulatedKeyStrokes.Count == 0) return;
        }

        // --- Check the next keystroke
        EmulatedKeyStroke keyStroke;
        lock (_emulatedKeyStrokes)
        {
            keyStroke = _emulatedKeyStrokes.Peek();
        }

        // --- Time has not come
        if (keyStroke.StartTact > Tacts) return;

        if (keyStroke.EndTact < Tacts)
        {
            // --- End emulation of this very keystroke
            KeyboardDevice.SetStatus(keyStroke.PrimaryCode, false);
            if (keyStroke.SecondaryCode.HasValue)
            {
                KeyboardDevice.SetStatus(keyStroke.SecondaryCode.Value, false);
            }
            lock (_emulatedKeyStrokes)
            {
                _emulatedKeyStrokes.Dequeue();
            }

            return;
        }

        // --- Emulate this very keystroke, and leave it in the queue
        KeyboardDevice.SetStatus(keyStroke.PrimaryCode, true);
        if (keyStroke.SecondaryCode.HasValue)
        {
            KeyboardDevice.SetStatus(keyStroke.SecondaryCode.Value, true);
        }
    }

    /// <summary>
    /// Adds an emulated keypress to the queue of the provider.
    /// </summary>
    /// <param name="startFrame">Frame count to start the emulation</param>
    /// <param name="frames">Number of frames to hold the emulation</param>
    /// <param name="primary">Primary key code</param>
    /// <param name="secondary">Optional secondary key code</param>
    /// <remarks>The provider can play back emulated key strokes</remarks>
    public override void QueueKeyPress(
        int startFrame, 
        int frames, 
        SpectrumKeyCode primary, 
        SpectrumKeyCode? secondary)
    {
        lock (_emulatedKeyStrokes)
        {
            var startTact = (ulong)startFrame * (ulong)TactsInFrame * (ulong)ClockMultiplier;
            var endTact = startTact + (ulong)frames * (ulong)TactsInFrame * (ulong)ClockMultiplier;
            var keypress = new EmulatedKeyStroke(startTact, endTact, primary, secondary);
            if (_emulatedKeyStrokes.Count == 0)
            {
                _emulatedKeyStrokes.Enqueue(keypress);
                return;
            }

            var last = _emulatedKeyStrokes.Peek();
            if (last.PrimaryCode == keypress.PrimaryCode
                && last.SecondaryCode == keypress.SecondaryCode)
            {
                // --- The same key has been clicked
                if (keypress.StartTact >= last.StartTact && keypress.StartTact <= last.EndTact)
                {
                    // --- Old and new click ranges overlap, lengthen the old click
                    last.EndTact = keypress.EndTact;
                    return;
                }
            }
            _emulatedKeyStrokes.Enqueue(keypress);
        }
    }

    #endregion

    /// <summary>
    /// The machine's execution loop calls this method when it is about to initialize a new frame.
    /// </summary>
    /// <param name="clockMultiplierChanged">
    /// Indicates if the clock multiplier has been changed since the execution of the previous frame.
    /// </param>
    protected override void OnInitNewFrame(bool clockMultiplierChanged)
    {
        // --- No screen tact rendered in this frame
        _lastRenderedFrameTact = 0;

        // --- Prepare the screen device for the new machine frame
        ScreenDevice.OnNewFrame();

        // --- Handle audio sample recalculations when the actual clock frequency changes
        if (_oldClockMultiplier != ClockMultiplier)
        {
            BeeperDevice.SetAudioSampleRate(AUDIO_SAMPLE_RATE);
            _oldClockMultiplier = ClockMultiplier;
        }

        // --- Prepare the beeper device for the new frame
        BeeperDevice.OnNewFrame();
    }

    /// <summary>
    /// Tests if the machine should raise a Z80 maskable interrupt
    /// </summary>
    /// <returns>
    /// True, if the INT signal should be active; otherwise, false.
    /// </returns>
    protected override bool ShouldRaiseInterrupt() => CurrentFrameTact / ClockMultiplier < 32;

    /// <summary>
    /// 
    /// </summary>
    protected override void AfterInstructionExecuted()
    {
        BeeperDevice.RenderBeeperSample();
        TapeDevice.UpdateTapeMode();
    }

    /// <summary>
    /// Every time the CPU clock is incremented with a single T-state, this function is executed.
    /// </summary>
    /// <param name="increment">The tact increment value</param>
    public override void OnTactIncremented(int increment)
    {
        var machineTact = CurrentFrameTact / ClockMultiplier;
        while (_lastRenderedFrameTact <= machineTact)
        {
            ScreenDevice.RenderTact(_lastRenderedFrameTact++);
        }
    }

    /// <summary>
    /// Uploades the specified ROM information to the ZX Spectrum 48 ROM memory
    /// </summary>
    /// <param name="data">ROM contents</param>
    private void UploadRomBytes(byte[] data)
    {
        for (var i = 0; i < data.Length; i++)
        {
            _memory[i] = data[i];
        }
    }
}
