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
        ob.Background.ShouldBe(OutputColors.Black);
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
    public void WriteLineWithEmptyContentsAndAction()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        
        ob.WriteLine("Hello!", _ => {});
        
        // --- Assert
        ob.Contents.Count.ShouldBe(2);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldNotBe(null);
        ob.Contents[1].ShouldBeEmpty();
    }

    [Fact]
    public void WriteLineWithAppendText()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        ob.Write("Hello!");
        
        // --- Act
        ob.WriteLine("World!");        
        
        // --- Assert
        ob.Contents.Count.ShouldBe(2);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!World!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
        ob.Contents[1].ShouldBeEmpty();
    }
    
    [Fact]
    public void WriteLineWithAppendTextAfterAction()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        ob.Write("Hello!", _ => { });
        
        // --- Act
        ob.WriteLine("World!");        
        
        // --- Assert
        ob.Contents.Count.ShouldBe(2);
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
        
        ob.Contents[1].ShouldBeEmpty();
    }
    
    [Fact]
    public void WriteLineWithAppendAction1()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        ob.Write("Hello!");
        
        // --- Act
        ob.WriteLine("World!", _ => { });        
        
        // --- Assert
        ob.Contents.Count.ShouldBe(2);
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
        
        ob.Contents[1].ShouldBeEmpty();
    }
    
    [Fact]
    public void WriteLineWithAppendAction2()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        ob.Write("Hello!", _ => { });
        
        // --- Act
        ob.WriteLine("World!", _ => { });        
        
        // --- Assert
        ob.Contents.Count.ShouldBe(2);
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
        
        ob.Contents[1].ShouldBeEmpty();
    }
    
    [Fact]
    public void WriteLineWithAppendAction3()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        ob.WriteLine("Hello!", _ => { });
        
        // --- Act
        ob.WriteLine("World!", _ => { });        
        
        // --- Assert
        ob.Contents.Count.ShouldBe(3);
        ob.Contents[0].Count.ShouldBe(1);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldNotBe(null);
        
        section = ob.Contents[1][0];
        section.Text.ShouldBe("World!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldNotBe(null);
        
        ob.Contents[2].ShouldBeEmpty();
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
        section.Background.ShouldBe(ob.Background);
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
        section.Background.ShouldBe(ob.Background);
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
        section.Background.ShouldBe(ob.Background);
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
    
    [Fact]
    public void WriteWithEmptyContentsAndBackColor1()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Background = OutputColors.Cyan;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(OutputColors.Cyan);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndBackColor2()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Background = OutputColors.Cyan;
        ob.Background = OutputColors.Red;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(OutputColors.Red);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndBackColor3()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Background = OutputColors.Cyan;
        ob.Write("Hello!");
        ob.Background = OutputColors.Red;
        ob.Write("World!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(2);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(OutputColors.Cyan);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
        
        section = ob.Contents[0][1];
        section.Text.ShouldBe("World!");
        section.Background.ShouldBe(OutputColors.Red);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }
    
    [Fact]
    public void WriteWithEmptyContentsAndBold1()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Bold = true;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(true);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndBold2()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Bold = false;
        ob.Bold = true;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(true);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndBold3()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Bold = false;
        ob.Write("Hello!");
        ob.Bold = true;
        ob.Write("World!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(2);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(false);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
        
        section = ob.Contents[0][1];
        section.Text.ShouldBe("World!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(true);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }
    
    [Fact]
    public void WriteWithEmptyContentsAndItalic1()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Italic = true;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(true);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndItalic2()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Italic = false;
        ob.Italic = true;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(true);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndItalic3()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Italic = false;
        ob.Write("Hello!");
        ob.Italic = true;
        ob.Write("World!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(2);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(false);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
        
        section = ob.Contents[0][1];
        section.Text.ShouldBe("World!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(true);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }
    
    [Fact]
    public void WriteWithEmptyContentsAndUnderline1()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Underline = true;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(true);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndUnderline2()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Underline = false;
        ob.Underline = true;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(true);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndUnderline3()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Underline = false;
        ob.Write("Hello!");
        ob.Underline = true;
        ob.Write("World!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(2);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(false);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
        
        section = ob.Contents[0][1];
        section.Text.ShouldBe("World!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(true);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
    }
    
    [Fact]
    public void WriteWithEmptyContentsAndStrikethrough1()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Strikethrough = true;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(true);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndStrikethrough2()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Strikethrough = false;
        ob.Strikethrough = true;
        ob.Write("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(true);
        section.Action.ShouldBe(null);
    }

    [Fact]
    public void WriteWithEmptyContentsAndStrikethrough3()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        ob.Strikethrough = false;
        ob.Write("Hello!");
        ob.Strikethrough = true;
        ob.Write("World!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(1);
        ob.Contents[0].Count.ShouldBe(2);
        
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(false);
        section.Action.ShouldBe(null);
        
        section = ob.Contents[0][1];
        section.Text.ShouldBe("World!");
        section.Color.ShouldBe(ob.Color);
        section.Background.ShouldBe(ob.Background);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(true);
        section.Action.ShouldBe(null);
    }
    
    [Fact]
    public void WriteLineWithEmptyContents()
    {
        // --- Arrange
        var ob = new OutputBuffer();
        
        // --- Act
        
        ob.WriteLine("Hello!");
        
        // --- Assert
        ob.Contents.Count.ShouldBe(2);
        ob.Contents[0].Count.ShouldBe(1);
        var section = ob.Contents[0][0];
        section.Text.ShouldBe("Hello!");
        section.Color.ShouldBe(ob.Color);
        section.Bold.ShouldBe(ob.Bold);
        section.Italic.ShouldBe(ob.Italic);
        section.Underline.ShouldBe(ob.Underline);
        section.Strikethrough.ShouldBe(ob.Strikethrough);
        section.Action.ShouldBe(null);
        
        ob.Contents[1].ShouldBeEmpty();
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
}