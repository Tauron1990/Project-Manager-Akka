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

    public IInputElement? RootInputElement { get; private set; }

    private void SetRootInput(IInputElement inputElement)
    {
        if(RootInputElement is null)
            RootInputElement = inputElement;
        else
            throw new InvalidOperationException("Only one Input Element can be Used");
    }
    
    public override void VisitGameTitle(GameTitleElement gameTitleElement)
        => AddWriter(static () => AnsiConsole.Write(new FigletText("Space Conqueror"){ Alignment = Justify.Center}));

    public override void Visit(RenderElement element)
    {
        RootInputElement = null;
        _frames.Clear();
        
        base.Visit(element);
    }

    protected override void NextCommandFrame(CommandFrame frame)
    {
        if(RootInputElement is null)
        {
            _currentFrame = frame;
            SetRootInput(frame);
            return;
        }
        
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
        => SetRootInput(new AskInputElement(askElement.Label));

    public override void VisitText(TextElement textElement)
        => AddWriter(() => AnsiConsole.WriteLine(textElement.Test));

    public override void VisitDocument(DocumentElement documentElement)
        => AddWriter(() => AnsiConsole.WriteLine(documentElement.Document.Render(documentElement.Context)));
}