using System;
using System.IO;
using SpectrumEngine.Emu;

namespace SpectrumEngine.Client.Avalonia.Providers;

/// <summary>
/// The default implementation of ITapeSaver, which saves the tape data into a TZX file.
/// </summary>
public class DefaultTapeSaver: ITapeSaver
{
    private const string DEFAULT_SAVE_FOLDER = "SavedFiles";
    private const string DEFAULT_NAME = "SavedFile";
    private const string DEFAULT_EXT = ".tzx";

    // --- Suggested fiel name 
    private string? _suggestedName;
    private string? _fullFileName;
    
    // --- Number of data blocks to save
    private int _dataBlockCount;

    /// <summary>
    /// This method sets the name of the file according to the 
    /// Spectrum SAVE HEADER information
    /// </summary>
    /// <param name="name">Name to set</param>
    public void SetName(string name)
    {
        _suggestedName = name;
    }

    /// <summary>
    /// Appends the TZX block to the tape file
    /// </summary>
    /// <param name="block">Tape block to save</param>
    public void SaveTapeBlock(TzxStandardSpeedBlock block)
    {
        // --- Get the folder name to save the file 
        if (_dataBlockCount == 0)
        {
            var homeFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var saveFolder = App.AppViewModel.Preferences.SaveFolder ?? DEFAULT_SAVE_FOLDER;
            var savePath = Path.IsPathFullyQualified(saveFolder)
                ? saveFolder
                : Path.Combine(homeFolder, saveFolder);
        
            // --- Take care the folder exists
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            // --- Get the target file name
            var baseFileName = $"{_suggestedName ?? DEFAULT_NAME}_{DateTime.Now:yyyyMMdd_HHmmss}{DEFAULT_EXT}";
            _fullFileName = Path.Combine(savePath, baseFileName);
            try
            {
                using var writer = new BinaryWriter(File.Create(_fullFileName));
                var header = new TzxHeader();
                header.WriteTo(writer);
            }
            catch
            {
                // --- Ignored intentionally
            }
        }
        _dataBlockCount++;

        if (_fullFileName == null) return;
        {
            var stream = File.Open(_fullFileName, FileMode.Append);
            using var writer = new BinaryWriter(stream);
            block.WriteTo(writer);
        }
    }
}