using Shouldly;
using SpectrumEngine.Tools.Disassembler;
using Xunit;

namespace SpectrumEngine.Tools.Test.Disassembler;

public class SpectrumSpecificTests
{
    [Fact]
    public void Rst08WorksAsExpected()
    {
        // --- Act
        Z80Tester.Test(SpectrumSpecificDisassemblyFlags.Spectrum48,
            new[] {"rst #08", ".defb #0a"}, 0xCF, 0x0A);
    }

    [Fact]
    public void Rst08GoesOnAsExpected()
    {
        // --- Act
        Z80Tester.Test(SpectrumSpecificDisassemblyFlags.Spectrum48,
            new[] {"rst #08", ".defb #0a", "nop"}, 0xCF, 0x0A, 0x00);
    }

    [Fact]
    public void Rst28GoesOnAsExpected()
    {
        // --- Act
        Z80Tester.Test(SpectrumSpecificDisassemblyFlags.Spectrum48,
            new[] {"rst #28", ".defb #38", "nop"}, 0xEF, 0x38, 0x00);
    }

    [Fact]
    public void Rst28SectionWorksAsExpected()
    {
        // --- Arrange
        var opCodes = new byte[] {0x02, 0xE1, 0x34, 0xF1, 0x38, 0xAA, 0x3B, 0x29, 0x00};
        var expected = new[]
        {
            ".defb #02",
            ".defb #e1",
            ".defb #34",
            ".defb #f1, #38, #aa, #3b, #29",
            "nop"
        };
        var expComment = new[]
        {
            "(delete)",
            "(get-mem-1)",
            "(stk-data)",
            "(1.442695)",
            null
        };

        // --- Act
        var map = new MemoryMap
        {
            new(0x0000, (ushort) (opCodes.Length - 2), MemorySectionType.Rst28Calculator),
            new((ushort) (opCodes.Length - 1), (ushort) (opCodes.Length - 1))
        };
        var disassembler = new Z80Disassembler(map, opCodes);
        var output = disassembler.Disassemble();

        // --- Assert
        output.OutputItems.Count.ShouldBe(expected.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            output.OutputItems[i].Instruction!.ToLower().ShouldBe(expected[i]);
        }

        output.OutputItems.Count.ShouldBe(expComment.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            output.OutputItems[i].HardComment?.ToLower().ShouldBe(expComment[i]);
        }
    }

}
