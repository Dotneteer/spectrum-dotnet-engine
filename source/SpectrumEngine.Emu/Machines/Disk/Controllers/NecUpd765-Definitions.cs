namespace SpectrumEngine.Emu.Machines.Disk.Controllers;

/// <summary>
/// Definitions file
/// </summary>
public partial class NecUpd765
{
    /// <summary>
    /// Current phase of the controller
    /// </summary>
    private enum Phase
    {
        /// <summary>
        /// FDC is in an idle state, awaiting the next initial command byte.
        /// </summary>
        Idle,
        /// <summary>
        /// The FDC receives all information required to perform a particular operation from the processor.
        /// </summary>
        Command,
        /// <summary>
        /// The FDC performs the operation it was instructed to do.
        /// </summary>
        Execution,
        /// <summary>
        /// After completion of the operation, status and other housekeeping information are made available to the processor.
        /// </summary>
        Result
    }

    /// <summary>
    /// Command directions
    /// </summary>
    private enum CommandDirection
    {
        /// <summary>
        /// Data flows from UPD765A to Z80
        /// </summary>
        Out,
        /// <summary>
        /// Data flows from Z80 to UPD765A
        /// </summary>
        In
    }

    /// <summary>
    /// Command parameters
    /// </summary>
    public enum CommandParameter
    {
        HEAD = 0,
        /// <summary>Track</summary>
        C = 1,
        /// <summary>Side</summary>
        H = 2,
        /// <summary>Sector ID</summary>
        R = 3,
        /// <summary>Sector size</summary>
        N = 4,
        /// <summary>End of track</summary>
        EOT = 5,
        /// <summary>Gap length</summary>
        GPL = 6,
        /// <summary>Data length</summary>
        DTL = 7,
        /// <summary>Step</summary>
        STP = 7,
    }

    /// <summary>
    /// Command result parameters
    /// </summary>
    public enum CommandResultParameter
    {
        /// <summary>Status register 0</summary>
        ST0 = 0,
        /// <summary>Status register 1</summary>
        ST1 = 1,
        /// <summary>Status register 2</summary>
        ST2 = 2,
        /// <summary>Track</summary>
        C = 3,
        /// <summary>Side</summary>
        H = 4,
        /// <summary>Sector ID</summary>
        R = 5,
        /// <summary>Sector size</summary>
        N = 6,
    }

    /// <summary>
    /// Main status registers
    /// </summary>
    public enum MainStatusRegister
    {
        /// <summary>
        /// FDD0 Busy
        /// FDD number 0 is in the seek mode. II any of the DnB bits IS set FDC will not accept read or write command.
        /// </summary>
        D0B = 0,
        /// <summary>
        /// FDD1 Busy
        /// FDD number 1 is in the seek mode. II any of the DnB bits IS set FDC will not accept read or write command.
        /// </summary>
        D1B = 1,
        /// <summary>
        /// FDD2 Busy
        /// FDD number 2 is in the seek mode. II any of the DnB bits IS set FDC will not accept read or write command.
        /// </summary>
        D2B = 2,
        /// <summary>
        /// FDD3 Busy
        /// FDD number 3 is in the seek mode. II any of the DnB bits IS set FDC will not accept read or write command.
        /// </summary>
        D3B = 3,
        /// <summary>
        /// FDC Busy
        /// A Read or Write command is in process. FDC will not accept any other command.
        /// </summary>
        CB = 4,
        /// <summary>
        /// Execution Mode
        /// This bit is set only during execution phase in non-DMA mode When DB5 goes low, execution phase has ended and result phase has started.
        /// It operates only during non-DMA mode of operation
        /// </summary>
        EXM = 5,
        /// <summary>
        /// Data Input/Output
        /// Indicates direction of data transfer between FDC and data register If DIO = 1, then transfer is from data register to the processor.
        /// If DIO = 0, then transfer is from the processor to data register
        /// </summary>
        DIO = 6,
        /// <summary>
        /// Request For Master
        /// Indicates data register IS ready to send or receive data to or from the processor.
        /// Both bits DIO and RQM should be used to perform the hand-shaking functions of "ready" and "directron" to the processor
        /// </summary>
        RQM = 7,
    }

    /// <summary>
    /// Status Register 0
    /// </summary>
    public enum StatusRegister0
    {
        /// <summary>
        /// Unit Select 0
        /// This flaa is used to Indicate a drive unit number at interrupt
        /// </summary>
        US0 = 0,
        /// <summary>
        /// Unit Select 1
        /// This flag is used to indicate a drive unit number at interrupt.
        /// </summary>
        US1 = 1,
        /// <summary>
        /// Head Address 
        /// This flag is used to indicate the state of the head at interrupt.
        /// </summary>
        HD = 2,
        /// <summary>
        /// Not Ready
        /// When the FDD IS in the not-ready state and a Read or Write command IS Issued, this flag is set If a Read or Write command is
        /// issued to side 1 of a single-sided drive, then this flag is set.
        /// </summary>
        NR = 3,
        /// <summary>
        /// Equipment Check
        /// If a fault srgnal IS received from the FDD, or if the track 0 signal fails to occur after 77 step pulses (Recalibrate Command) then this flag is set.
        /// </summary>
        EC = 4,
        /// <summary>
        /// Seek End
        /// When the FDC completes the Seek command, this flag IS set lo 1 (high).
        /// </summary>
        SE = 5,
        /// <summary>
        /// Interrupt Code (D6)
        /// D7=0 and D6=0 => Normal termination of command, (NT) Command was completed and properly executed.
        /// D7=0 and D6=1 => Abnormal termination of command, (AT) Execution of command was started but was not successfully completed.
        /// D7=1 and D6=0 => Invalid command issue, (IC) Command which was issued was never started.
        /// D7=1 and D6=1 => Abnormal termination because during command execution the ready srgnal from FDD changed state.
        /// </summary>
        IC_D6 = 6,
        /// <summary>
        /// Interrupt Code (D7)
        /// D7=0 and D6=0 => Normal termination of command, (NT) Command was completed and properly executed.
        /// D7=0 and D6=1 => Abnormal termination of command, (AT) Execution of command was started but was not successfully completed.
        /// D7=1 and D6=0 => Invalid command issue, (IC) Command which was issued was never started.
        /// D7=1 and D6=1 => Abnormal termination because during command execution the ready srgnal from FDD changed state.
        /// </summary>
        IC_D7 = 7,
    }

    /// <summary>
    /// Status Register 1
    /// </summary>
    public enum StatusRegister1
    {
        /// <summary>
        /// Missing Address Mark
        /// This bit is set if the FDC does not detect the IDAM before 2 index pulses It is also set if
        /// the FDC cannot find the DAM or DDAM after the IDAM is found. MD bit of ST2 is also ser at this time.
        /// </summary>
        MA = 0,
        /// <summary>
        /// Not Writeable
        /// During execution of Write Data, Write Deleted Data or Write ID command. if the FDC.
        /// detect: a write protect srgnal from the FDD. then this flag is Set.
        /// </summary>
        NW = 1,
        /// <summary>
        /// No Data
        /// During execution of Read Data. Read Deleted Data Write Data. Write Deleted Data or Scan command, 
        /// if the FDC cannot find the sector specified in the IDR(2) Register, this flag is set.
        /// </summary>
        ND = 2,
        /// <summary>
        /// Over Run
        /// If the FDC i s not serviced by the host system during data transfers within a certain time interval. this flaa i s set.
        /// </summary>
        OR = 4,
        /// <summary>
        /// Data Error
        /// f the FDC i s not serviced by the host system during data transfers within a certain time interval. this flaa i s set.
        /// </summary>
        DE = 5,
        /// <summary>
        /// End of Track
        /// hen the FDC tries to access a sector beyond the final sector of a cylinder, this flag is set.
        /// </summary>
        EN = 7,
    }

    /// <summary>
    /// Status Register 2
    /// </summary>
    public enum StatusRegister2
    {
        /// <summary>
        /// Missing Address Mark in Data Field
        /// When data IS read from the medium, if the FDC cannot find a data address mark or 
        /// deleted data address mark, then this flag is set.
        /// </summary>
        MD = 0,
        /// <summary>
        /// Bad Cylinder
        /// This bit i s related to the ND bit. and when the contents of C on the medium is different from 
        /// that stored in the IDR and the contents of C is FFH. then this flag is set.
        /// </summary>
        BC = 1,
        /// <summary>
        /// Scan Not Satisfied
        /// During execution of the Scan command, if the FD cannot find a sector on the cylinder which meets the condition.
        /// then this flag is set.
        /// </summary>
        SN = 2,
        /// <summary>
        /// Scan Equal Hit
        /// During execution of the Scan command. if the condition of "equal" is satisfied, this flag is set.
        /// </summary>
        SH = 3,
        /// <summary>
        /// Wrong Cylinder
        /// This bit is related to the ND bit, and when the contents of C(3) on the medium is different from that stored i n the IDR.
        /// this flag is set.
        /// </summary>
        WC = 4,
        /// <summary>
        /// Data Error in Data Field
        /// If the FDC detects a CRC error in the data field then this flag is set.
        /// </summary>
        DD = 5,
        /// <summary>
        /// Control Mark
        /// During execution of the Read Data or Scan command, if the FDC encounters a sector
        /// which contains a deleted data address mark, this flag is set Also set if DAM is found during Read Deleted Data.
        /// </summary>
        CM = 6,
    }

    /// <summary>
    /// Status Register 3
    /// </summary>
    public enum StatusRegister3
    {
        /// <summary>
        /// Unit select 0
        /// This bit is used to indicate the status of the unit select 0 signal to the FDD.
        /// </summary>
        US0 = 0,
        /// <summary>
        /// Unit select 1
        /// This bit is used to Indicate the status of the unit select 1 signal to the FDD.
        /// </summary>
        US1 = 1,
        /// <summary>
        /// Head address 
        /// This bit is used to indicate the status of the ide select signal to the FDD.
        /// </summary>
        HD = 2,
        /// <summary>
        /// Two Side (0 = yes, 1 = no)
        /// This bit is used to indicate the status of the two-side signal from the FDD.
        /// </summary>
        TS = 3,
        /// <summary>
        /// Track 0
        /// This bit is used to indicate the status of the track 0 signal from the FDD.
        /// </summary>
        T0 = 4,
        /// <summary>
        /// Ready
        /// This bit is used to Indicate the status of the ready signal from the FDD.
        /// </summary>
        RY = 5,
        /// <summary>
        /// Write Protected
        /// This bit is used to indicate the status of the write protected signal from the FDD.
        /// </summary>
        WP = 6,
        /// <summary>
        /// Fault
        /// This bit is used to indicate the status of the fault signal from the FDD.
        /// </summary>
        FT = 7,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum DriveSeekState
    {
        Idle = 0,
        Seek = 1,
        Recalibrate = 2,
    }
}
