namespace SpectrumEngine.Emu;

/// <summary>
/// Represents frame statistics about the last running frame
/// </summary>
public class FrameStats
{
    /// <summary>
    /// Number of frames rendered since last cold start
    /// </summary>
    public int FrameCount { get; set; }
    
    /// <summary>
    /// Time required for the last CPU frame in milliseconds
    /// </summary>
    public double LastCpuFrameTimeInMs { get; set; }
    
    /// <summary>
    /// Average CPU frame time since last cold start in milliseconds
    /// </summary>
    public double AvgCpuFrameTimeInMs { get; set; }
    
    /// <summary>
    /// Time required to render the last frame in milliseconds
    /// </summary>
    public double LastFrameTimeInMs { get; set; }
    
    /// <summary>
    /// Average time to render the frames since last cold start in milliseconds
    /// </summary>
    public double AvgFrameTimeInMs { get; set; }
}