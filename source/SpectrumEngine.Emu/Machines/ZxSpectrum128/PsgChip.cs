namespace SpectrumEngine.Emu.Machines.ZxSpectrum128;

/// <summary>
/// Represents a PSG chip
/// </summary>
public class PsgChip
{
    // --- The last register index set
    private int _psgRegisterIndex;
    
    // --- The last values of the PSG registers set
    private readonly byte[] _regValues = new byte[16];

    // --- Stores the envelopes volume forms
    private readonly byte[] _psgEnvelopes = new byte[0x800];
    
    // --- Table of volume levels
    private readonly ushort[] _psgVolumeTable = {
        0x0000, 0x0201, 0x033c, 0x04d7,
        0x0783, 0x0ca6, 0x133e, 0x2393,
        0x2868, 0x45d4, 0x606a, 0x76ea,
        0x97bc, 0xb8a6, 0xdc52, 0xffff
    };

    // --- Channel A
    private ushort _toneA;
    private bool _toneAEnabled;
    private bool _noiseAEnabled;
    private byte _volA;
    private bool _envA;
    private ushort _cntA;
    private bool _bitA;

    // --- Channel B
    private ushort _toneB;
    private bool _toneBEnabled;
    private bool _noiseBEnabled;
    private byte _volB;
    private bool _envB;
    private ushort _cntB;
    private bool _bitB;

    // --- Channel C
    private ushort _toneC;
    private bool _toneCEnabled;
    private bool _noiseCEnabled;
    private byte _volC;
    private bool _envC;
    private ushort _cntC;
    private bool _bitC;

    // --- Noise
    private uint _noiseSeed;
    private ushort _noiseFreq;
    private ushort _cntNoise;
    private bool _bitNoise;

    // --- Envelope data
    private ushort _envFreq;
    private byte _envStyle;
    private ushort _cntEnv;
    private ushort _posEnv;

    /// <summary>
    /// Sum of orphan samples
    /// </summary>
    public int OrphanSum { get; set; }
    
    /// <summary>
    /// Number of orphan samples
    /// </summary>
    public int OrphanSamples { get; set; }

    public PsgChip()
    {
        InitSoundRegisters();
        InitEnvelopData();
    }

    /// <summary>
    /// Set the initial values of all sound registers and their internal representation
    /// </summary>
    private void InitSoundRegisters()
    {
        // --- Set all previous register values to zero
        for (var i = 0; i < _regValues.Length; i++) _regValues[i] = 0;

        // --- Channel A setup
        _toneA = 0;
        _toneAEnabled = false;
        _noiseAEnabled = false;
        _volA = 0;
        _envA = false;
        _cntA = 0;
        _bitA = false;

        // --- Channel B setup
        _toneB = 0;
        _toneBEnabled = false;
        _noiseBEnabled = false;
        _volB = 0;
        _envB = false;
        _cntB = 0;
        _bitB = false;

        // --- Channel C setup
        _toneC = 0;
        _toneCEnabled = false;
        _noiseCEnabled = false;
        _volC = 0;
        _envC = false;
        _cntC = 0;
        _bitC = false;

        // --- Noise channel setup
        _noiseSeed = 0;
        _noiseFreq = 0;
        _cntNoise = 0;
        _bitNoise = false;

        // --- Other registers
        _envFreq = 0;
        _envStyle = 0;
        _cntEnv = 0;
        _posEnv = 0;
    }

    /// <summary>
    /// Initialize the PSG envelope tables
    /// </summary>
    private void InitEnvelopData()
    {
        // Reset the sample pointer
        var samplePtr = 0;

        // --- Iterate through envelopes
        for (var env = 0; env < 16; env++)
        {
            // --- Reset hold
            var hold = false;

            // --- Set dir according to env
            var dir = (env & 0x04) != 0 ? 1 : -1;

            // --- Set vol according to env
            var vol = (env & 0x04) != 0 ? -1 : 0x20;

            // --- Iterate through envelope positions
            for (var pos = 0; pos < 128; pos++)
            {
                if (!hold) {
                    vol += dir;
                    if (vol is < 0 or >= 32) {
                        // -- Continue flag is set?
                        if ((env & 0x08) != 0) {
                            // --- Yes, continue.
                            // --- If alternate is set, reverse the direction
                            if ((env & 0x02) != 0) {
                                dir = -dir;
                            }

                            // --- Set start volume according to direction
                            vol = dir > 0 ? 0 : 31;

                            // --- Hold is set?
                            if ((env & 0x01) != 0) {
                                // --- Hold, and set up next volume
                                hold = true;
                                vol = dir > 0 ? 31 : 0;
                            }
                        } else {
                            // --- Mute and hold this value
                            vol = 0;
                            hold = true;
                        }
                    }
                }

                // --- Store the envelop sample and move to the next position
                _psgEnvelopes[samplePtr++] = (byte)vol;
            }
        }
    }

    /// <summary>
    /// Set the PSG register index
    /// </summary>
    /// <param name="index">PSG register index (0-15)</param>
    public void SetPsgRegisterIndex(int index)
    {
        _psgRegisterIndex = index & 0x0f;
    }

    /// <summary>
    /// Reads the value of the register addressed by the register index last set
    /// </summary>
    /// <returns>PSG register value</returns>
    public byte ReadPsgRegisterValue() => _regValues[_psgRegisterIndex & 0x0f];

    /// <summary>
    /// Writes the value of the register addressed by the register index last set
    /// </summary>
    /// <param name="v"></param>
    public void WritePsgRegisterValue(byte v)
    {
        // --- Write the native register values
        _regValues[_psgRegisterIndex] = v;

        switch (_psgRegisterIndex)
        {
            case 0:
                // --- Tone A (lower 8 bits)
                _toneA = (ushort)((_toneA & 0x0f00) | v);
                return;
            
            case 1:
                // --- Tone A (upper 4 bits)
                _toneA = (ushort)((_toneA & 0x00ff) | ((v & 0x0f) << 8));
                return;
            
            case 2:
                // --- Tone B (lower 8 bits)
                _toneB = (ushort)((_toneB & 0x0f00) | v);
                return;

            case 3:
                // --- Tone B (upper 4 bits)
                _toneB = (ushort)((_toneB & 0x00ff) | ((v & 0x0f) << 8));
                return;
            
            case 4:
                // --- Tone C (lower 8 bits)
                _toneC = (ushort)((_toneC & 0x0f00) | v);
                return;
            
            case 5:
                // --- Tone C (upper 4 bits)
                _toneC = (ushort)((_toneC & 0x00ff) | ((v & 0x0f) << 8));
                return;
                
            case 6:
                // --- Noise frequency
                _noiseFreq = (ushort)(v & 0x1f);
                return;
            
            case 7:
                // --- Mixer flags
                _toneAEnabled = (v & 0x01) == 0;
                _toneBEnabled = (v & 0x02) == 0;
                _toneCEnabled = (v & 0x04) == 0;
                _noiseAEnabled = (v & 0x08) == 0;
                _noiseBEnabled = (v & 0x10) == 0;
                _noiseCEnabled = (v & 0x20) == 0;
                return;
            
            case 8:
                // --- Volume A
                _volA = (byte)(v & 0x0f);
                _envA = (v & 0x10) != 0;
                return;
            
            case 9:
                // --- Volume B
                _volB = (byte)(v & 0x0f);
                _envB = (v & 0x10) != 0;
                return;

            case 10:
                // --- Volume C
                _volC = (byte)(v & 0x0f);
                _envC = (v & 0x10) != 0; 
                return;
            
            case 11:
                // --- Envelope fequency (lower 8 bit)
                _envFreq = (ushort)((_envFreq & 0xff00) | v);
                return;

            case 12:
                // --- Envelope frequency (upper 8 bits)
                _envFreq = (ushort)((_envFreq & 0x00ff) | (v << 8));
                return;
            
            case 13:
                // --- Check envelope shape
                _envStyle = (byte)(v & 0x0f);
                _cntEnv = 0;
                _posEnv = 0;
                return;
        }
    }

    /// <summary>
    /// Generates the current PSG output value
    /// </summary>
    public void GenerateOutputValue()
    {
        var vol = 0;
  
        // --- Increment TONE A counter
        _cntA++;
        if (_cntA >= _toneA)
        {
            // --- Reset counter and reverse output bit
            _cntA = 0;
            _bitA = !_bitA;
        }

        // --- Increment TONE B counter
        _cntB++;
        if (_cntB >= _toneB) {
            // --- Reset counter and reverse output bit
            _cntB = 0;
            _bitB = !_bitB;
        }

        // --- Increment TONE C counter
        _cntC++;
        if (_cntC >= _toneC) {
            // --- Reset counter and reverse output bit
            _cntC = 0;
            _bitC = !_bitC;
        }

        // --- Calculate noise sample
        _cntNoise++;
        if (_cntNoise >= _noiseFreq) 
        {
            // --- It is time to generate the next noise sample
            _cntNoise = 0;
            _noiseSeed = (_noiseSeed * 2 + 1) ^ (((_noiseSeed >> 16) ^ (_noiseSeed >> 13)) & 0x01);
            _bitNoise = ((_noiseSeed >> 16) & 0x01) != 0;
        }

        // --- Calculate envelope position
        _cntEnv++;
        if (_cntEnv >= _envFreq)
        {
            // --- Move to the new position
            _cntEnv = 0;
            _posEnv++;
            if (_posEnv > 0x7f) 
            {
                _posEnv = 0x40;
            }
        }

        // --- Add Channel A volume value
        int tmpVol;
        if ((_bitA && _toneAEnabled) || (_bitNoise && _noiseAEnabled))
        {
            if (_envA) 
            {
                tmpVol = _psgEnvelopes[_envStyle * 128 + _posEnv];
            } 
            else
            {
                tmpVol = _volA * 2 + 1;
            }

            // --- At this point tmpVol is 0-31, let's convert it to 0-65535
            vol += _psgVolumeTable[(tmpVol & 0x1f) >> 1];
        }

        // --- Add Channel B volume value
        if ((_bitB && _toneBEnabled) || (_bitNoise && _noiseBEnabled)) 
        {
            if (_envB) 
            {
                tmpVol = _psgEnvelopes[_envStyle * 128 + _posEnv];
            } 
            else
            {
                tmpVol = _volB * 2 + 1;
            }

            // --- At this point tmpVol is 0-31, let's convert it to 0-65535
            vol += _psgVolumeTable[(tmpVol & 0x1f) >> 1];
        }

        // --- Add Channel C volume value
        if ((_bitC && _toneCEnabled) || (_bitNoise && _noiseCEnabled)) 
        {
            if (_envC) 
            {
                tmpVol = _psgEnvelopes[_envStyle * 128 + _posEnv];
            } 
            else 
            {
                tmpVol = _volC * 2 + 1;
            }

            // --- At this point tmpVol is 0-31, let's convert it to 0-65535
            vol += _psgVolumeTable[(tmpVol & 0x1f) >> 1];
        }

        OrphanSum += vol;
        OrphanSamples += 1;
    }
}