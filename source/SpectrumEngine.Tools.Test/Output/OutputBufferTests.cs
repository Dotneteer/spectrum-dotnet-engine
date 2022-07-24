using Shouldly;
using SpectrumEngine.Tools.Output;
using Xunit;
// ReSharper disable UseObjectOrCollectionInitializer

namespace SpectrumEngine.Tools.Test.Output;

public class CommandParserTests
{
    [Fact]
    public void ContentsEmptyAfterConstruction()
    {
        // --- Act
        var ob = new OutputBuffer();
        
        // --- Assert
        ob.Contents.Count.ShouldBe(0);
        ob.Color.ShouldBe(OutputColors.White);
        ob.BackColor.ShouldBe(OutputColors.Black);
        ob.Bold.ShouldBeFalse();
        ob.Italic.ShouldBeFalse();
        ob.Underline.ShouldBeFalse();
        ob.Strikethrough.ShouldBeFalse();
        ob.GetLastSection().ShouldBeNull();
    }
    
    [Fact]
    public void WriteWithEmptyContents()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndAction()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        
        ob.Write("Hello!", _ => {});
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldNotBe(null);
    }

    [Fact]
    public void WriteWithAppendText()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        ob.Write("Hello!");
        
        // --- Act
        ob.Write("World!");        
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!World!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }
    
    [Fact]
    public void WriteWithAppendTextAfterAction()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        ob.Write("Hello!", _ => { });
        
        // --- Act
        ob.Write("World!");        
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(2);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldNotBe(null);
        
        section = ob.Contents[0][1];
        section.Text.ShouldBe("World!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }
    
    [Fact]
    public void WriteWithAppendAction1()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        ob.Write("Hello!");
        
        // --- Act
        ob.Write("World!", _ => { });        
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(2);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
        
        section = ob.Contents[0][1];
        section.Text.ShouldBe("World!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldNotBe(null);
    }
    
    [Fact]
    public void WriteWithAppendAction2()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        ob.Write("Hello!", _ => { });
        
        // --- Act
        ob.Write("World!", _ => { });        
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(2);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldNotBe(null);
        
        section = ob.Contents[0][1];
        section.Text.ShouldBe("World!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldNotBe(null);
    }
    
    [Fact]
    public void WriteWithEmptyContentsAndColor1()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Color = OutputColors.Cyan;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(OutputColors.Cyan);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndColor2()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Color = OutputColors.Cyan;
        ob.Color = OutputColors.Red;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(OutputColors.Red);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndColor3()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Color = OutputColors.Cyan;
        ob.Write("Hello!");
        ob.Color = OutputColors.Red;
        ob.Write("World!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(2);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(OutputColors.Cyan);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
        
        section = ob.Contents[0][1];
        section.Text.ShouldBe("World!");
        section.Color.ShouldBe(OutputColors.Red);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }
}