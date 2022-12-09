using Spectre.Console;
using Tauron.TextAdventure.Engine.Console;
using Tauron.TextAdventure.Engine.UI.Rendering;

namespace SpaceConqueror.UI;

public sealed class ConsoleUIVisitor : SpectreVisitor
{
    private readonly Stack<CommandFrame> _frames = new();
    private CommandFrame? _currentFrame;

    private CommandFrame CurrentFrame
    {
        get
        {
            if(_currentFrame is null)
                throw new InvalidOperationException("No Current Frame Setted");

            return _currentFrame;
        }
        set => _currentFrame = value;
    }

    public CommandFrame RootCommandFrame { get; set; }
    
    public ConsoleUIVisitor()
        => RootCommandFrame = new CommandFrame();

    public override void VisitGameTitle(GameTitleElement gameTitleElement)
        => AddWriter(static () => AnsiConsole.Write(new FigletText("Space Conqueror"){ Alignment = Justify.Center}));

    public override void Visit(RenderElement element)
    {
        RootCommandFrame = new CommandFrame();
        _currentFrame = RootCommandFrame;
        _frames.Clear();
        
        base.Visit(element);
    }

    protected override void NextCommandFrame(CommandFrame frame)
    {
        _frames.Push(CurrentFrame);
        CurrentFrame.AddFrame(frame);
        CurrentFrame = frame;
    }

    protected override void CommandFrameExit()
        => CurrentFrame = _frames.Pop();

    public override void VisitCommandItem(CommandItem commandItem)
        => CurrentFrame.AddItem(commandItem);

    public override void VisitSpacing(SpacingElement spacingElement)
    {
        for (int i = 0; i < spacingElement.Amount; i++)
        {
            AddWriter(AnsiConsole.WriteLine);
        }
    }

    public override void VisitAsk(AskElement askElement)
    {
        throw new NotImplementedException();
    }

    public override void VisitText(TextElement textElement)
        => AddWriter(() => AnsiConsole.WriteLine(textElement.Test));
}