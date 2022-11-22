﻿using SpectrumEngine.Emu.Abstractions;
using SpectrumEngine.Emu.Extensions;
using SpectrumEngine.Emu.Machines.Disk.Controllers;
using SpectrumEngine.Emu.Machines.Disk.FloppyDisks;
using System;

namespace SpectrumEngine.Emu.Machines.Disk;

/// <summary>
/// Floppy disk drive cluster
/// </summary>
public class FlopyDiskDriveCluster
{
    private readonly Z80MachineBase _machine;
    private readonly NecUpd765 _floppyDiskController;
    private readonly IFlopyDiskDriveDevice[] _floppyDiskDrives = new FlopyDiskDriveDevice[2];

    private int _floppyDiskDriveSlot = 0;

    public FlopyDiskDriveCluster(Z80MachineBase machine)
    {
        _machine = machine;
        _floppyDiskController = new NecUpd765(machine, this);

        Reset();
    }

    /// <summary>
    /// Signs whether is motor running 
    /// </summary>
    public bool IsMotorRunning => _floppyDiskController.FlagMotor;

    /// <summary>
    /// Signs whether is disk inserted in active drive slot
    /// </summary>
    public bool IsFloppyDiskLoaded => ActiveFloppyDiskDrive != null && ActiveFloppyDiskDrive.IsDiskLoaded;

    /// <summary>
    /// Active floppy disk
    /// </summary>
    public FloppyDisk? FloppyDisk => ActiveFloppyDiskDrive?.Disk;

    /// <summary>
    /// Currently active floppy drive device
    /// </summary>
    public IFlopyDiskDriveDevice? ActiveFloppyDiskDrive { get; private set; }

    /// <summary>
    /// Active floppy disk drive slot
    /// </summary>
    public int FloppyDiskDriveSlot
    {
        get => _floppyDiskDriveSlot;
        set
        {
            _floppyDiskDriveSlot = value;
            ActiveFloppyDiskDrive = _floppyDiskDrives[_floppyDiskDriveSlot];
        }
    }

    /// <summary>
    /// Load floppy disk data in active slot
    /// </summary>
    /// <exception cref="InvalidOperationException">Invalid disk data</exception>
    public void LoadDisk(byte[] diskData)
    {
        ActiveFloppyDiskDrive?.LoadDisk(diskData);
    }

    /// <summary>
    /// Ejects floppy disk from active slot
    /// </summary>
    public void EjectDisk()
    {
        ActiveFloppyDiskDrive?.EjectDisk();
    }

    /// <summary>
    /// Initialization / reset of the floppy drive subsystem
    /// </summary>
    private void Reset()
    {
        for (int i = 0; i < 4; i++)
        {
            _floppyDiskDrives[i] = new FlopyDiskDriveDevice(i, _floppyDiskController);
        }

        FloppyDiskDriveSlot = 0;
    }   
}
