# The Beeper Device Implementation

The `BeeperDevice` class is responsible for creating sound samples from the changes of the EAR bit (the beeper output bit). This device implements the `IBeeperDevice` interface defining the behavior:

```csharp
public interface IBeeperDevice: IAudioDevice
{
    // --- This method sets the EAR bit value to generate sound with the beeper.
    void SetEarBit(bool value);

    // Renders the subsequent beeper sample according to the current EAR bit value
    void RenderBeeperSample();
}
```

`IBeeperDevice` derives from a more generic interface, `IAudioDevice`, which represents any audio device that generates sound samples. For example, the ZX Spectrum 128K and upper models have an AY-3-8912 Programmable Sound Generator chip that also creates sound samples besides the beeper.

```csharp
public interface IAudioDevice : IGenericDevice<IZxSpectrum48Machine>
{
    // --- Sets up the sample rate to use with this device
    void SetAudioSampleRate(int sampleRate);

    // --- Gets the audio samples rendered in the current frame
    float[] GetAudioSamples();

    // --- This method signs that a new machine frame has been started
    void OnNewFrame();
}
```

## The Essential Members of the Beeper Device

The interfaces above declare a few properties and methods:
- `SetAudioSampleRate` allows preparing the device for sound sample generation. This method uses the current clock frequency and the clock multiplier to calculate the number of samples to create in a machine frame.
- The UI can invoke the `GetAudioSamples` method to obtain the sound samples generated in a particular machine frame.
- As the number of samples within a single machine frame is generally not an integer, the `OnNewFrame` method takes care to optionally generate the last sample in a completed frame.
- Whenever the code sets the EAR bit, the `SetEarBit` method of the beeper device administers it; this value determines the subsequent sound sample added.
- The `RenderBeeperSample` is responsible for generating the subsequent sound sample.

## Sound Samples

Let's do some basic math to determine the highest sound frequency we can generate with the ZX Spectrum's beeper! We must send alternating EAR bits to the beeper output to generate a sound pulse. The fastest way of doing this is similar to this code:

```
ld a,$10
out ($fe),a
ld a,$00
out ($fe),a
```

Let's forget that the code also sets the border value. The essential thing is that the subsequent OUT instructions alter Bit 4, the EAR bit.
The four instructions take 36 clock cycles to generate a single sound pulse. Using the 3.5MHz clock frequency results in 3,500,000 ÷ 36 = 97,227 Hz (less than 100 kHz) theoretical maximum frequency.

The average human ear can hear up to 20 kHz, so we do not have to prepare for such a high frequency when determining the sound sampling rate. The current implementation of the beeper device uses 48,000 samples/second rate.
 
The beeper device uses the `SetAudioSampleRate` method (see in the `BeeperDevice` class) to set up its state for sound sample generation:

```csharp
public void SetAudioSampleRate(int sampleRate)
{
    var sampleLength = (double)Machine.BaseClockFrequency * Machine.ClockMultiplier / sampleRate;
    _audioSampleLength = (int)sampleLength;
    _audioLowerGate = (int)((sampleLength - _audioSampleLength) * GATE);
    _audioGateValue = 0;
}
```

The `sampleLength` value calculated within this method shows the number of clock cycles between two sound samples, and it is rarely an integer number. For example, using a 48,000 bit/sec sample rate and the 3.5MHz clock frequency, `sampleLength` is 72.9166. This value means that the distance between sound samples is 72 or 73 (because of the fractional part, more often 73 than 72).

To determine when to use 72 or 73, we use a simple technique often utilized by graphics algorithms to handle fractional values without fractional operations.

- When calculating the number of samples, we use a value, `GATE` (100,000), and another, `_audioLowerGate` (91,667), calculated from the fractional part of `sampleLength`.
- We store the integer part of `sampleLength` (72) as the initial value of distance between samples.
- We accumulate the sound sample length from the start of the machine in the `_audioGateValue` variable, which is initially zero.

The algorithm that generates the samples stores the *next* tact within a machine frame in the `_audioNextSampleTact` member variable. When it's time to create the subsequent sample, the `RenderBeeperSample` calculates not only the sample but also updates `_audioNextSampleTact` to prepare for the next call:

```csharp
public void RenderBeeperSample()
{
    if (Machine.CurrentFrameTact <= _audioNextSampleTact) return;
        
    _audioSamples.Add(_earBitValue ? 1.0f : 0.0f);
    _audioGateValue += _audioLowerGate;
    _audioNextSampleTact += _audioSampleLength;
    if (_audioGateValue < GATE) return;
        
    _audioNextSampleTact += 1;
    _audioGateValue -= GATE;
}
```

Observe that we continuously increase the `_audioGateValue` variable with `_audioLowerGate` (91,667). When the accumulated value is less than `GATE` (100,000), we use 72 as a distance to the next sample tact; otherwise, 73.

Here are  the values of `_audioGateValue`, `_audioNextSampleTact`, (and  the sample distance) for the first few sound samples:

```csharp
91_667,   72 (72)
83_334,  145 (73)
75_001,  218 (73)
66_668,  291 (73)
58_335,  364 (73)
50_002,  437 (73)
41_669,  510 (73)
33_336,  583 (73)
25_003,  656 (73)
16_670,  729 (73)
 8_337,  802 (73)
     4,  875 (73)
91_671,  947 (72)
83_338, 1020 (73)
```

Machine frames end when Z80 instructions have been entirely completed;  thus, they do not always end precisely at the last tact of the physical frame: the previous frame may overflow into the next one. The `OnNewFrame` method takes care to handle this overflow; it creates the last sound sample if that would happen during the last Z80 instruction (the one that causes the frame overflow) and updates `_audioNextSampleTact`:

```csharp
public void OnNewFrame()
{
    var cpuTactsInFrame = Machine.TactsInFrame * Machine.ClockMultiplier;
    if (_audioNextSampleTact != 0)
    {
        if (_audioNextSampleTact > cpuTactsInFrame)
        {
            _audioNextSampleTact -= cpuTactsInFrame;
        }
        else
        {
            _audioSamples.Add(_earBitValue ? 1.0f : 0.0f);
            _audioNextSampleTact = _audioSampleLength - cpuTactsInFrame + _audioNextSampleTact;
        }
    }
    _audioSamples.Clear();
}
```

## Integrating the Beeper Device with the Machine Frame

The machine has a virtual method, `OnTactIncremented`, defined in `IZ80Cpu`. The CPU invokes this method whenever it increments the clock cycle count (tact). The `ZxSpectrum48Machine` class overrides `OnTactIncremented` to handle screen and sound generation:

```csharp
public override void OnTactIncremented(int increment)
{
    var machineTact = CurrentFrameTact / ClockMultiplier;
    while (_lastRenderedFrameTact <= machineTact)
    {
        ScreenDevice.RenderTact(_lastRenderedFrameTact++);
    }
    
    // --- This is where the next sound sample is generated
    BeeperDevice.RenderBeeperSample();
}
```

The engine invokes the SetAudioSampleRate method in two locations within `ZxSpectrum48Machine`:
- In the `Reset`, when it emulates a soft or hard reset.
- In the `OnInitNewFrame` method if the clock multiplier value has changed, as it needs to recalculate the sample length.
