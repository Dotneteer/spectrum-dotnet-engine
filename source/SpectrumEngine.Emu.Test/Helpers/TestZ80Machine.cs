namespace SpectrumEngine.Emu.Test;

/// <summary>
/// This class implements a Z80 machine that can be used for unit testing.
/// </summary>
/// <remarks>
/// The methods of the class allow injecting and running Z80 code. Helper methods make it easy to test expected
/// behavior.
/// </remarks>
public class Z80TestMachine
{
    private readonly Stack<ushort> _stepOutStack = new Stack<ushort>();
    private bool _callExecuted;
    private bool _retExecuted;

    /// <summary>
    /// The CPU of the test machine
    /// </summary>
    public Z80Cpu Cpu { get; }

    /// <summary>
    /// Defines the run modes the Z80TestMachine allows
    /// </summary>
    public RunMode RunMode { get; protected set; }

    /// <summary>
    /// The operative memory of the test machine
    /// </summary>
    public byte[] Memory { get; }

    /// <summary>
    /// The address where the code execution ends.
    /// </summary>
    public ushort CodeEndsAt { get; private set; }

    /// <summary>
    /// A log that helps testing memory access operations.
    /// </summary>
    public List<MemoryOp> MemoryAccessLog { get; }

    /// <summary>
    /// A log that helps testing I/O access operations.
    /// </summary>
    public List<IoOp> IoAccessLog { get; }

    /// <summary>
    /// We can define the I/O input expected in a particular test scenario with this sequence.
    /// </summary>
    public List<byte> IoInputSequence { get; }

    /// <summary>
    /// The count of I/O reads
    /// </summary>
    public int IoReadCount { get; private set; }

    /// <summary>
    /// Sign that a CPU cycle has just been completed.
    /// </summary>
    public event EventHandler? CpuCycleCompleted;

    /// <summary>
    /// Store the values of the Z80 registers before a test case runs.
    /// </summary>
    public Z80Cpu.Registers? RegistersBeforeRun { get; private set; }

    /// <summary>
    /// Store the state of the memory before a test case runs.
    /// </summary>
    public byte[]? MemoryBeforeRun { get; private set; }

    /// <summary>
    /// Helper information to test the step-out functionality (CALL instructions)
    /// </summary>
    public List<ushort> CallStepOutEvents = new();

    /// <summary>
    /// Helper information to test the step-out functionality (RET instructions)
    /// </summary>
    public List<ushort> RetStepOutEvents = new();

    /// <summary>
    /// Helper information to test the step-out functionality (PUSH instructions)
    /// </summary>
    public List<ushort> StepOutPushEvents = new();

    /// <summary>
    /// Helper information to test the step-out functionality (POP instructions)
    /// </summary>
    public List<ushort> StepOutPopEvents = new();

    /// <summary>
    /// Initialize the test machine.
    /// </summary>
    /// <param name="runMode">Specify the mode in which the test machine runs.</param>
    /// <param name="allowExtendedInstructions">Sign if ZX Spectrum Next extended instructions can run.</param>
    public Z80TestMachine(RunMode runMode = RunMode.Normal, bool allowExtendedInstructions = false)
    {
        Memory = new byte[ushort.MaxValue + 1];
        MemoryAccessLog = new List<MemoryOp>();
        IoAccessLog = new List<IoOp>();
        IoInputSequence = new List<byte>();
        Cpu = new Z80Cpu
        {
            AllowExtendedInstructions = allowExtendedInstructions,
            ReadMemoryFunction = ReadMemory,
            WriteMemoryFunction = WriteMemory,
            ReadPortFunction = ReadPort,
            WritePortFunction = WritePort
        };
        RunMode = runMode;
    }

    /// <summary>
    /// Initializes the code passed in <paramref name="programCode"/>. This code
    /// is put into the memory from <paramref name="codeAddress"/> and
    /// code execution starts at <paramref name="startAddress"/>
    /// </summary>
    /// <param name="programCode">Bytes of the program</param>
    /// <param name="codeAddress">Injection start address</param>
    /// <param name="startAddress">Execution start address</param>
    public void InitCode(IEnumerable<byte>? programCode = null, ushort codeAddress = 0,
        ushort startAddress = 0)
    {
        // --- Initialize the code
        if (programCode != null)
        {
            foreach (var op in programCode)
            {
                Memory[codeAddress++] = op;
            }
            CodeEndsAt = codeAddress;
            while (codeAddress != 0)
            {
                Memory[codeAddress++] = 0;
            }
        }

        // --- Init code execution
        Cpu.Reset();
        Cpu.Regs.PC = startAddress;
    }

    /// <summary>
    /// Run the injected code.
    /// </summary>
    public void Run()
    {
        RegistersBeforeRun = CloneZ80Registers();
        MemoryBeforeRun = new byte[ushort.MaxValue + 1];
        Memory.CopyTo(MemoryBeforeRun, 0);
        var stopped = false;

        while (!stopped)
        {
            Cpu.ExecuteCpuCycle();
            CpuCycleCompleted?.Invoke(this, EventArgs.Empty);
            stopped = RunMode switch
            {
                RunMode.OneCycle => true,
                RunMode.OneInstruction => Cpu.Prefix == Z80Cpu.OpCodePrefix.None,
                RunMode.UntilHalt => (Cpu.SignalFlags & Z80Cpu.Z80Signals.Halted) != 0,
                RunMode.UntilEnd => Cpu.Regs.PC >= CodeEndsAt,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    /// <summary>
    /// This method reads a byte from the memory.
    /// </summary>
    /// <param name="addr">Memory address</param>
    /// <returns>Data byte read from the memory</returns>
    /// <remarks>
    /// Override in derived classes to define a different memory read operation.
    /// </remarks>
    protected virtual byte ReadMemory(ushort addr)
    {
        var value = Memory[addr];
        MemoryAccessLog.Add(new MemoryOp(addr, value, false));
        return value;
    }

    /// <summary>
    /// This method writes a byte into the memory.
    /// </summary>
    /// <param name="addr">Memory address</param>
    /// <param name="value">Byte value to write</param>
    /// <remarks>
    /// Override in derived classes to define a different memory write operation.
    /// </remarks>
    protected virtual void WriteMemory(ushort addr, byte value)
    {
        Memory[addr] = value;
        MemoryAccessLog.Add(new MemoryOp(addr, value, true));
    }

    /// <summary>
    /// This method reads a byte from an I/O port.
    /// </summary>
    /// <param name="addr">I/O port address</param>
    /// <returns>Data byte read from the I/O port</returns>
    /// <remarks>
    /// Override in derived classes to define a different I/O read operation.
    /// </remarks>
    protected virtual byte ReadPort(ushort addr)
    {
        var value = IoReadCount >= IoInputSequence.Count
            ? (byte)0x00
            : IoInputSequence[IoReadCount++];
        IoAccessLog.Add(new IoOp(addr, value, false));
        return value;
    }

    /// <summary>
    /// This method writes a byte into an I/O port
    /// </summary>
    /// <param name="addr">I/O port address</param>
    /// <param name="value">Byte value to write</param>
    /// <remarks>
    /// Override in derived classes to define a different I/O port write operation.
    /// </remarks>
    protected virtual void WritePort(ushort addr, byte value)
    {
        IoAccessLog.Add(new IoOp(addr, value, true));
    }

    /// <summary>
    /// Clone the current set of Z80 registers
    /// </summary>
    /// <returns>The cloned register set</returns>
    private Z80Cpu.Registers CloneZ80Registers()
    {
        var regs = Cpu.Regs;
        return new Z80Cpu.Registers()
        {
            _AF_ = regs._AF_,
            _BC_ = regs._BC_,
            _DE_ = regs._DE_,
            _HL_ = regs._HL_,
            AF = regs.AF,
            BC = regs.BC,
            DE = regs.DE,
            HL = regs.HL,
            SP = regs.SP,
            PC = regs.PC,
            IX = regs.IX,
            IY = regs.IY,
            IR = regs.IR,
            WZ = regs.WZ
        };
    }

    /// <summary>
    /// This class stores information about memory access operations.
    /// </summary>
    public class MemoryOp
    {
        /// <summary>
        /// Memory address accessed during the operation
        /// </summary>
        public ushort Address { get; }

        /// <summary>
        /// Value read or written
        /// </summary>
        public byte Value { get; }

        /// <summary>
        /// Was the operation a memory write?
        /// </summary>
        public bool IsWrite { get; }

        /// <summary>
        /// Initializes the operation from the arguments
        /// </summary>
        public MemoryOp(ushort address, byte values, bool isWrite)
        {
            Address = address;
            Value = values;
            IsWrite = isWrite;
        }
    }

    /// <summary>
    /// This class stores information about I/O port access operations.
    /// </summary>
    public class IoOp
    {
        /// <summary>
        /// I/O port address accessed during the operation
        /// </summary>
        public ushort Address { get; }

        /// <summary>
        /// Value read or written
        /// </summary>
        public byte Value { get; }

        /// <summary>
        /// Was the operation an I/O port write?
        /// </summary>
        public bool IsOutput { get; }

        /// <summary>
        /// Initializes the operation from the arguments
        /// </summary>
        public IoOp(ushort address, byte value, bool isOutput)
        {
            Address = address;
            Value = value;
            IsOutput = isOutput;
        }
    }

    /// <summary>
    /// Checks if the Step-Out stack contains any information
    /// </summary>
    /// <returns></returns>
    public bool HasStepOutInfo()
    {
        return _stepOutStack.Count > 0;
    }

    /// <summary>
    /// The depth of the Step-Out stack
    /// </summary>
    public int StepOutStackDepth => _stepOutStack.Count;

    /// <summary>
    /// Clears the content of the Step-Out stack
    /// </summary>
    public void ClearStepOutStack()
    {
        _stepOutStack.Clear();
    }

    /// <summary>
    /// Pushes the specified return address to the Step-Out stack
    /// </summary>
    /// <param name="address"></param>
    public void PushStepOutAddress(ushort address)
    {
        _stepOutStack.Push(address);
        StepOutPushEvents.Add(address);
    }

    /// <summary>
    /// Pops a Step-Out return point address from the stack
    /// </summary>
    /// <returns>Address popped from the stack</returns>
    /// <returns>Zeor, if the Step-Out stack is empty</returns>
    public ushort PopStepOutAddress()
    {
        if (_stepOutStack.Count > 0)
        {
            StepOutAddress = _stepOutStack.Pop();
            StepOutPopEvents.Add(StepOutAddress.Value);
            return StepOutAddress.Value;
        }
        StepOutAddress = null;
        StepOutPopEvents.Add(0);
        return 0;
    }

    /// <summary>
    /// Indicates that the last instruction executed by the CPU was a CALL
    /// </summary>
    public bool CallExecuted
    {
        get => _callExecuted;
        set
        {
            _callExecuted = value;
            if (value)
            {
                CallStepOutEvents.Add(Cpu.Regs.PC);
            }
        }
    }

    /// <summary>
    /// Indicates that the last instruction executed by the CPU was a RET
    /// </summary>
    public bool RetExecuted
    {
        get => _retExecuted;
        set
        {
            _retExecuted = value;
            if (value)
            {
                RetStepOutEvents.Add(Cpu.Regs.PC);
            }
        }
    }

    /// <summary>
    /// Gets the last popped Step-Out address
    /// </summary>
    public ushort? StepOutAddress { get; set; }
}
