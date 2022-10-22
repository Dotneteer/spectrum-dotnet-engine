namespace SpectrumEngine.Emu;

/// <summary>
/// The common core functionality for all ZX Spectrum machines 
/// </summary>
public abstract class ZxSpectrumBase: Z80MachineBase, IZxSpectrumMachine
{
    #region Private members

    protected const int AUDIO_SAMPLE_RATE = 48_000;

    // --- This byte array stores the contention values associated with a particular machine frame tact.
    private byte[] _contentionValues = Array.Empty<byte>();

    // --- Last value of bit 3 on port $FE
    private bool _portBit3LastValue;

    // --- Last value of bit 4 on port $FE
    private bool _portBit4LastValue;

    // --- Tacts value when last time bit 4 of $fe changed from 0 to 1
    private ulong _portBit4ChangedFrom0Tacts;

    // --- Tacts value when last time bit 4 of $fe changed from 1 to 0
    private ulong _portBit4ChangedFrom1Tacts;

    // --- Stores the key strokes to emulate
    protected readonly Queue<EmulatedKeyStroke> EmulatedKeyStrokes = new();

    #endregion

    #region Initialization and Properties

    /// <summary>
    /// The clock multiplier value used in the previous machine frame
    /// </summary>
    protected int OldClockMultiplier;

    /// <summary>
    /// Stores the last rendered machine frame tact.
    /// </summary>
    protected int LastRenderedFrameTact;

    /// <summary>
    /// Represents the keyboard device of ZX Spectrum 48K
    /// </summary>
    public IKeyboardDevice KeyboardDevice { get; protected init; } = null!;

    /// <summary>
    /// Represents the screen device of ZX Spectrum 48K
    /// </summary>
    public IScreenDevice ScreenDevice { get; protected init; } = null!;

    /// <summary>
    /// Represents the beeper device of ZX Spectrum 48K
    /// </summary>
    public IBeeperDevice BeeperDevice { get; protected init; } = null!;

    /// <summary>
    /// Represents the floating port device of ZX Spectrum 48K
    /// </summary>
    public IFloatingBusDevice FloatingBusDevice { get; protected init; } = null!;

    /// <summary>
    /// Represents the tape device of ZX Spectrum 48K
    /// </summary>
    public ITapeDevice TapeDevice { get; protected init; } = null!;

    /// <summary>
    /// Indicates if the currently selected ROM is the ZX Spectrum 48 ROM
    /// </summary>
    public virtual bool IsSpectrum48RomSelected => true;

    /// <summary>
    /// Reads the screen memory byte
    /// </summary>
    /// <param name="offset">Offset from the beginning of the screen memory</param>
    /// <returns>The byte at the specified screen memory location</returns>
    public abstract byte ReadScreenMemory(ushort offset);

    /// <summary>
    /// Get the 64K of addressable memory of the ZX Spectrum computer
    /// </summary>
    /// <returns>Bytes of the flat memory</returns>
    public abstract byte[] Get64KFlatMemory();

    /// <summary>
    /// Get the specified 16K partition (page or bank) of the ZX Spectrum computer
    /// </summary>
    /// <param name="index">Partition index</param>
    /// <returns>Bytes of the partition</returns>
    /// <remarks>
    /// Less than zero: ROM pages
    /// 0..7: RAM bank with the specified index
    /// </remarks>
    public abstract byte[] Get16KPartition(int index);

    /// <summary>
    /// Gets the audio sample rate
    /// </summary>
    public abstract int GetAudioSampleRate();

    /// <summary>
    /// Gets the audio samples rendered in the current frame
    /// </summary>
    /// <returns>Array with the audio samples</returns>
    public abstract float[] GetAudioSamples();

    /// <summary>
    /// Get the number of T-states in a display line (use -1, if this info is not available)
    /// </summary>
    public override int TactsInDisplayLine => ScreenDevice.ScreenWidth;

    #endregion

    #region Memory Device

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
        TotalContentionDelaySinceStart += 3;
        ContentionDelaySincePause += 3;
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
    public override void DelayMemoryWrite(ushort address) => DelayMemoryRead(address);

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
        if ((address & 0xc000) != 0x4000) return;
        
        // --- We read from contended memory
        var delay = _contentionValues[CurrentFrameTact];
        TactPlusN(delay);
        TotalContentionDelaySinceStart += delay;
        ContentionDelaySincePause += delay;
    }

    /// <summary>
    /// Gets the ULA issue number of the ZX Spectrum model (2 or 3)
    /// </summary>
    public virtual int UlaIssue { get; set; } = 3;

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
    /// This function implements the I/O port read delay of the CPU.
    /// </summary>
    /// <remarks>
    /// Normally, it is exactly 4 T-states; however, it may be higher in particular hardware. If you do not set your
    /// action, the Z80 CPU will use its default 4-T-state delay. If you use custom delay, take care that you increment
    /// the CPU tacts at least with 4 T-states!
    /// </remarks>
    public override void DelayPortRead(ushort address) => DelayContendedIo(address);

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
    protected byte ReadPort0Xfe(ushort address)
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
            if (!bit4Sensed)
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
    protected void WritePort0xFE(byte value)
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
    protected void DelayContendedIo(ushort address)
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
        
        TotalContentionDelaySinceStart += 4;
        ContentionDelaySincePause += 4;

        // --- Apply I/O contention
        void ApplyContentionDelay()
        {
            var delay = GetContentionValue(CurrentFrameTact);
            TactPlusN(delay);
            TotalContentionDelaySinceStart += delay;
            ContentionDelaySincePause += delay;
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
        lock (EmulatedKeyStrokes)
        {
            if (EmulatedKeyStrokes.Count == 0) return;
        }

        // --- Check the next keystroke
        EmulatedKeyStroke keyStroke;
        lock (EmulatedKeyStrokes)
        {
            keyStroke = EmulatedKeyStrokes.Peek();
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

            // --- Remove the keystroke from the queue
            lock (EmulatedKeyStrokes) EmulatedKeyStrokes.Dequeue();
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
    public override void QueueKeystroke(
        int startFrame, 
        int frames, 
        SpectrumKeyCode primary, 
        SpectrumKeyCode? secondary)
    {
        lock (EmulatedKeyStrokes)
        {
            var startTact = (ulong)startFrame * (ulong)TactsInFrame * (ulong)ClockMultiplier;
            var endTact = startTact + (ulong)frames * (ulong)TactsInFrame * (ulong)ClockMultiplier;
            var keypress = new EmulatedKeyStroke(startTact, endTact, primary, secondary);
            if (EmulatedKeyStrokes.Count == 0)
            {
                EmulatedKeyStrokes.Enqueue(keypress);
                return;
            }

            var last = EmulatedKeyStrokes.Peek();
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
            EmulatedKeyStrokes.Enqueue(keypress);
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
        LastRenderedFrameTact = 0;

        // --- Prepare the screen device for the new machine frame
        ScreenDevice.OnNewFrame();

        // --- Handle audio sample recalculations when the actual clock frequency changes
        if (OldClockMultiplier != ClockMultiplier)
        {
            BeeperDevice.SetAudioSampleRate(AUDIO_SAMPLE_RATE);
            OldClockMultiplier = ClockMultiplier;
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
    protected override bool ShouldRaiseInterrupt() => CurrentFrameTact < 32;

    /// <summary>
    /// Check for current tape mode
    /// </summary>
    protected override void AfterInstructionExecuted()
    {
        TapeDevice.UpdateTapeMode();
    }

    /// <summary>
    /// Every time the CPU clock is incremented, this function is executed.
    /// </summary>
    /// <param name="increment">The tact increment value</param>
    public override void OnTactIncremented(int increment)
    {
        var machineTact = CurrentFrameTact;
        while (LastRenderedFrameTact <= machineTact)
        {
            ScreenDevice.RenderTact(LastRenderedFrameTact++);
        }
        BeeperDevice.SetNextAudioSample();
    }
}