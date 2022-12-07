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
    
    public CommandFrame Root { get; private set; }

    public ConsoleUIVisitor()
        => Root = new CommandFrame();

    public override void VisitGameTitle(GameTitleElement gameTitleElement)
        => AddWriter(static () => AnsiConsole.Write(new FigletText("Space Conqueror"){ Alignment = Justify.Center}));

    public override void Visit(RenderElement element)
    {
        Root = new CommandFrame();
        _currentFrame = Root;
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
}