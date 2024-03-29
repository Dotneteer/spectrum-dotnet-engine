using System.Collections.Generic;
using System.Linq;
using Shouldly;
using SpectrumEngine.Tools.Disassembler;

namespace SpectrumEngine.Tools.Test.Disassembler;

public static class Z80Tester
{
    public static void Test(string expected, params byte[] opCodes)
    {
        var map = new MemoryMap
        {
            new(0x0000, (ushort) (opCodes.Length - 1))
        };
        var disassembler = new Z80Disassembler(map, opCodes);
        var output = disassembler.Disassemble();
        output.OutputItems.Count.ShouldBe(1);
        var item = output.OutputItems[0];
        item.Instruction!.ToLower().ShouldBe(expected.ToLower());
        item.LastAddress.ShouldBe((ushort) (opCodes.Length - 1));
        item.OpCodes.Trim().ShouldBe(string.Join(" ", opCodes.Select(o => $"{o:X2}")));
    }

    public static void TestExt(string expected, params byte[] opCodes)
    {
        var map = new MemoryMap
        {
            new(0x0000, (ushort) (opCodes.Length - 1))
        };
        var disassembler = new Z80Disassembler(map, opCodes, null, true);
        var output = disassembler.Disassemble();
        output.OutputItems.Count.ShouldBe(1);
        var item = output.OutputItems[0];
        item.Instruction!.ToLower().ShouldBe(expected.ToLower());
        item.LastAddress.ShouldBe((ushort) (opCodes.Length - 1));
        item.OpCodes.Trim().ShouldBe(string.Join(" ", opCodes.Select(o => $"{o:X2}")));
    }

    public static void Test(SpectrumSpecificDisassemblyFlags flags, string[] expected, params byte[] opCodes)
    {
        var map = new MemoryMap
        {
            new(0x0000, (ushort) (opCodes.Length - 1))
        };
        var disassembler = new Z80Disassembler(map, opCodes,
            new Dictionary<int, SpectrumSpecificDisassemblyFlags> {{0, flags}});
        var output = disassembler.Disassemble();
        output.OutputItems.Count.ShouldBe(expected.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            output.OutputItems[i].Instruction!.ToLower().ShouldBe(expected[i]);
        }
    }
}
