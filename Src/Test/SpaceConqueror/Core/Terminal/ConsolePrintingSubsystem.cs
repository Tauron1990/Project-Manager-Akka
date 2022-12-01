using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using Heretic.InteractiveFiction.Objects;
using Heretic.InteractiveFiction.Resources;
using Heretic.InteractiveFiction.Subsystems;
using Spectre.Console;
using Console = Spectre.Console.AnsiConsole;

namespace SpaceConqueror.Core.Terminal;

  public sealed class ConsolePrintingSubsystem : IPrintingSubsystem
  {
    private readonly AssetManager _assetManager;
    
    private int consoleWidth;

    public ConsolePrintingSubsystem(AssetManager assetManager)
    {
      Heretic.InteractiveFiction.Objects.Universe
      _assetManager = assetManager;
    }

    public int ConsoleWidth
    {
      get => consoleWidth == 0 ? System.Console.WindowWidth : Math.Min(System.Console.WindowWidth, this.consoleWidth);
      set => consoleWidth = value;
    }

    public TextColor ForegroundColor
    {
      get => (TextColor) Color.ToConsoleColor(Console.Foreground);
      set => Console.Foreground = Color.FromConsoleColor((ConsoleColor) value);
    }

    public TextColor BackgroundColor
    {
      get => (TextColor) Console.BackgroundColor;
      set => Console.BackgroundColor = (ConsoleColor) value;
    }

    public void ResetColors() => Console.ResetColor();

    public virtual bool ActiveLocation(
      Location activeLocation,
      IDictionary<Location, IEnumerable<Heretic.InteractiveFiction.Objects.DestinationNode>> locationMap)
    {
      Console.Write(this.WordWrap((object) activeLocation, this.ConsoleWidth));
      this.DestinationNode(activeLocation, locationMap);
      return true;
    }

    public virtual bool ActivePlayer(Player activePlayer)
    {
      Console.Write(this.WordWrap((object) activePlayer, this.ConsoleWidth));
      return true;
    }

    public virtual bool AlterEgo(AHereticObject item)
    {
      if (item == null)
      {
        Console.Write(this.WordWrap(BaseDescriptions.ITEM_NOT_VISIBLE, this.ConsoleWidth));
        Console.WriteLine();
      }
      else
        Console.WriteLine(this.WordWrap(item.AlterEgo(), this.ConsoleWidth));
      return true;
    }

    public virtual bool AlterEgo(string itemName)
    {
      if (string.IsNullOrEmpty(itemName))
      {
        Console.Write(this.WordWrap(BaseDescriptions.ITEM_NOT_VISIBLE, this.ConsoleWidth));
        Console.WriteLine();
      }
      else
      {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.AppendFormat(BaseDescriptions.ALTER_EGO_DESCRIPTION, (object) this.GetObjectName(itemName));
        stringBuilder.AppendLine(string.Join(", ", itemName.Split('|')));
        Console.WriteLine(this.WordWrap(stringBuilder.ToString(), this.ConsoleWidth));
      }
      return true;
    }

    public virtual bool CanNotUseObject(string objectName)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ITEM_UNKNOWN, this.ConsoleWidth), (object) objectName);
      Console.WriteLine();
      return true;
    }

    public virtual void ClearScreen() => Console.Clear();

    public virtual bool PrintObject(AHereticObject item)
    {
      if (item == null)
      {
        Console.Write(this.WordWrap(BaseDescriptions.ITEM_NOT_VISIBLE, this.ConsoleWidth));
        Console.WriteLine();
      }
      else
        Console.Write(this.WordWrap((object) item, this.ConsoleWidth));
      return true;
    }

    public virtual bool Help(IList<Verb> verbs)
    {
      this.GeneralHelp();
      this.VerbGroupDirections(verbs);
      this.VerbTalks(verbs);
      this.VerbInteractItems(verbs);
      this.VerbContainers(verbs);
      this.VerbMetaInfos(verbs);
      return true;
    }

    private void VerbHelp(
      IDictionary<string, IEnumerable<string>> verbResource)
    {
      foreach (KeyValuePair<string, IEnumerable<string>> keyValuePair in (IEnumerable<KeyValuePair<string, IEnumerable<string>>>) verbResource)
      {
        Console.Write(BaseDescriptions.ResourceManager.GetString(keyValuePair.Key));
        int num = 0;
        this.ForegroundColor = TextColor.Magenta;
        foreach (string str in keyValuePair.Value)
        {
          Console.Write(num != 0 ? ", " : "...");
          Console.Write(str);
          ++num;
        }
        this.ResetColors();
        Console.WriteLine();
      }
    }

    private IDictionary<string, IEnumerable<string>> GetDirectionVerbs(IList<Verb> verbs)
    {
      Collection<string> verbKeys = new Collection<string>()
                                    {
                                      "N",
                                      "S",
                                      "E",
                                      "W",
                                      "NE",
                                      "NW",
                                      "SE",
                                      "SW",
                                      "UP",
                                      "DOWN",
                                      "GO",
                                      "WAYS"
                                    };
      return (IDictionary<string, IEnumerable<string>>) BaseConsolePrintingSubsystem.FilterVerbs(verbs, verbKeys);
    }

    private static Dictionary<string, IEnumerable<string>> FilterVerbs(
      IList<Verb> verbs,
      Collection<string> verbKeys)
    {
      return verbs.Where<Verb>((Func<Verb, bool>) (x => verbKeys.Contains(x.Key))).OrderBy<Verb, string>((Func<Verb, string>) (x => x.Key)).ToDictionary<Verb, string, IEnumerable<string>>((Func<Verb, string>) (x => x.Key), (Func<Verb, IEnumerable<string>>) (x => x.Names));
    }

    protected void VerbGroupDirections(IList<Verb> verbs)
    {
      Console.WriteLine(HelpDescriptions.VERBS_DIRECTIONS);
      Console.WriteLine(new string('-', HelpDescriptions.VERBS_DIRECTIONS.Length));
      this.VerbHelp(this.GetDirectionVerbs(verbs));
      this.VerbGroupDirectionsExamples();
    }

    private void VerbGroupDirectionsExamples()
    {
      Console.WriteLine(HelpDescriptions.EXAMPLES);
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_DIRECTIONS_EXAMPLE_I);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_DIRECTIONS_EXAMPLE_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_DIRECTIONS_EXAMPLE_II);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_DIRECTIONS_EXAMPLE_DESCRIPTION);
      Console.WriteLine();
    }

    private IDictionary<string, IEnumerable<string>> GetMetaInfoVerbs(IList<Verb> verbs)
    {
      Collection<string> verbKeys = new Collection<string>()
      {
        "INV",
        "CREDITS",
        "SCORE",
        "ALTER_EGO",
        "HISTORY",
        "NAME",
        "HELP",
        "HINT",
        "REM",
        "SAVE",
        "QUIT"
      };
      return (IDictionary<string, IEnumerable<string>>) BaseConsolePrintingSubsystem.FilterVerbs(verbs, verbKeys);
    }

    protected void VerbMetaInfos(IList<Verb> verbs)
    {
      Console.WriteLine(HelpDescriptions.VERBS_METAINFO);
      Console.WriteLine(new string('-', HelpDescriptions.VERBS_METAINFO.Length));
      this.VerbHelp(this.GetMetaInfoVerbs(verbs));
      this.VerbMetaInfosExamples();
    }

    private void VerbMetaInfosExamples()
    {
      Console.WriteLine(HelpDescriptions.EXAMPLES);
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_METAINFO_EXAMPLE_I);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_METAINFO_EXAMPLE_I_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_METAINFO_EXAMPLE_II);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_METAINFO_EXAMPLE_II_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_METAINFO_EXAMPLE_III);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_METAINFO_EXAMPLE_II_DESCRIPTION);
      Console.WriteLine();
    }

    private IDictionary<string, IEnumerable<string>> GetInteractVerbs(IList<Verb> verbs)
    {
      Collection<string> verbKeys = new Collection<string>()
      {
        "TAKE",
        "DROP",
        "PULL",
        "PUSH",
        "BUY",
        "BREAK",
        "USE",
        "LOOK",
        "SIT",
        "STANDUP",
        "JUMP",
        "CLIMB",
        "DESCEND",
        "WAIT",
        "WRITE"
      };
      return (IDictionary<string, IEnumerable<string>>) BaseConsolePrintingSubsystem.FilterVerbs(verbs, verbKeys);
    }

    protected void VerbInteractItems(IList<Verb> verbs)
    {
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS);
      Console.WriteLine(new string('-', HelpDescriptions.VERBS_INTERACT_ITEMS.Length));
      this.VerbHelp(this.GetInteractVerbs(verbs));
      this.VerbInteractItemsExamples();
    }

    private void VerbInteractItemsExamples()
    {
      Console.WriteLine(HelpDescriptions.EXAMPLES);
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_I);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_I_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_II);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_II_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_III);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_III_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_IV);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_IV_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_V);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_INTERACT_ITEMS_EXAMPLE_V_DESCRIPTION);
      Console.WriteLine();
    }

    private IDictionary<string, IEnumerable<string>> GetTalkVerbs(IList<Verb> verbs)
    {
      Collection<string> verbKeys = new Collection<string>()
      {
        "TALK",
        "SAY",
        "ASK",
        "GIVE"
      };
      return (IDictionary<string, IEnumerable<string>>) BaseConsolePrintingSubsystem.FilterVerbs(verbs, verbKeys);
    }

    protected void VerbTalks(IList<Verb> verbs)
    {
      Console.WriteLine(HelpDescriptions.VERBS_TALKS);
      Console.WriteLine(new string('-', HelpDescriptions.VERBS_TALKS.Length));
      this.VerbHelp(this.GetTalkVerbs(verbs));
      this.VerbTalksExamples();
    }

    private void VerbTalksExamples()
    {
      Console.WriteLine(HelpDescriptions.EXAMPLES);
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_TALK_EXAMPLE_I);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_TALK_EXAMPLE_I_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_TALK_EXAMPLE_II);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_TALK_EXAMPLE_II_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_TALK_EXAMPLE_III);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_TALK_EXAMPLE_III_DESCRIPTION);
      Console.WriteLine();
    }

    private IDictionary<string, IEnumerable<string>> GetContainerVerbs(IList<Verb> verbs)
    {
      Collection<string> verbKeys = new Collection<string>()
      {
        "CLOSE",
        "OPEN",
        "UNLOCK",
        "LOCK",
        "TURN"
      };
      return (IDictionary<string, IEnumerable<string>>) BaseConsolePrintingSubsystem.FilterVerbs(verbs, verbKeys);
    }

    protected void VerbContainers(IList<Verb> verbs)
    {
      Console.WriteLine(HelpDescriptions.VERBS_CONTAINER);
      Console.WriteLine(new string('-', HelpDescriptions.VERBS_CONTAINER.Length));
      this.VerbHelp(this.GetContainerVerbs(verbs));
      this.VerbContainersExamples();
    }

    private void VerbContainersExamples()
    {
      Console.WriteLine(HelpDescriptions.EXAMPLES);
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_CONTAINER_EXAMPLE_I);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_CONTAINER_EXAMPLE_I_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.VERBS_CONTAINER_EXAMPLE_II);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.VERBS_CONTAINER_EXAMPLE_II_DESCRIPTION);
      Console.WriteLine();
    }

    protected void GeneralHelp()
    {
      Console.WriteLine(HelpDescriptions.HELP_DESCRIPTION);
      Console.WriteLine(new string('-', HelpDescriptions.HELP_DESCRIPTION.Length));
      Console.Write(HelpDescriptions.HELP_GENERAL_INSTRUCTION_PART_I);
      this.ForegroundColor = TextColor.Green;
      Console.Write(" " + this.GetObjectName(Verbs.LOOK).ToLower());
      this.ResetColors();
      Console.WriteLine(".");
      Console.WriteLine(HelpDescriptions.EXAMPLES);
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.HELP_GENERAL_INSTRUCTION_LOOK);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.ROOM_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.HELP_GENERAL_INSTRUCTION_LOOK_II);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.ITEM_DESCRIPTION);
      Console.WriteLine();
      Console.Write(HelpDescriptions.PROMPT);
      this.ForegroundColor = TextColor.Green;
      Console.WriteLine(HelpDescriptions.HELP_GENERAL_INSTRUCTION_LOOK_III);
      this.ResetColors();
      Console.WriteLine(HelpDescriptions.ITEM_DESCRIPTION_SHORT);
      Console.WriteLine();
      Console.WriteLine(HelpDescriptions.VERBS);
      Console.WriteLine(new string('-', HelpDescriptions.VERBS.Length));
    }

    public virtual bool History(ICollection<string> historyCollection)
    {
      StringBuilder message = new StringBuilder(historyCollection.Count);
      message.AppendJoin<string>(Environment.NewLine, (IEnumerable<string>) historyCollection);
      Console.WriteLine(this.WordWrap((object) message, this.ConsoleWidth));
      return true;
    }

    public virtual bool ItemNotVisible() => this.Resource(BaseDescriptions.ITEM_NOT_VISIBLE);

    public bool KeyNotVisible() => this.Resource(BaseDescriptions.KEY_NOT_VISIBLE);

    public virtual bool ImpossiblePickup(AHereticObject containerObject)
    {
      if (containerObject != null)
      {
        if (!string.IsNullOrEmpty((string) containerObject.UnPickAbleDescription))
          return this.Resource((string) containerObject.UnPickAbleDescription);
        if (containerObject is Character)
          return this.Resource(BaseDescriptions.IMPOSSIBLE_CHARACTER_PICKUP);
      }
      return this.Resource(this.GetRandomPhrase(BaseDescriptions.IMPOSSIBLE_PICKUP));
    }

    public virtual bool ItemToHeavy() => this.Resource(BaseDescriptions.TO_HEAVY);

    public virtual bool ItemPickupSuccess(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ITEM_PICKUP, this.ConsoleWidth), (object) item.AccusativeArticleName.LowerFirstChar());
      Console.WriteLine();
      return true;
    }

    public virtual bool ImpossibleDrop(AHereticObject item)
    {
      if (!string.IsNullOrEmpty((string) item.UnDropAbleDescription))
        return this.Resource((string) item.UnDropAbleDescription);
      Console.Write(this.WordWrap(BaseDescriptions.IMPOSSIBLE_DROP, this.ConsoleWidth), (object) item.AccusativeArticleName.LowerFirstChar());
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemAlreadyClosed(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ALREADY_CLOSED, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemClosed(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.NOW_CLOSED, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public bool ItemStillClosed(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ITEM_STILL_CLOSED, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public bool ItemAlreadyBroken(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ALREADY_BROKEN, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemAlreadyOpen(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ALREADY_OPEN, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemAlreadyUnlocked(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ALREADY_UNLOCKED, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public bool ItemUnbreakable(AHereticObject item)
    {
      if (string.IsNullOrEmpty((string) item.UnbreakableDescription))
        Console.Write(this.WordWrap(BaseDescriptions.ITEM_UNBREAKABLE, this.ConsoleWidth), (object) item.Name);
      else
        Console.Write(this.WordWrap((string) item.UnbreakableDescription, this.ConsoleWidth));
      Console.WriteLine();
      return true;
    }

    public bool ItemSeated(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ITEM_SEATED, this.ConsoleWidth), (object) item.DativeArticleName.LowerFirstChar());
      Console.WriteLine();
      return true;
    }

    public bool ItemNotSeatable(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ITEM_NOT_SEATABLE, this.ConsoleWidth), (object) item.AccusativeArticleName.LowerFirstChar());
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemStillLocked(AHereticObject item)
    {
      Console.Write(!string.IsNullOrEmpty((string) item.LockDescription) ? this.WordWrap((string) item.LockDescription, this.ConsoleWidth) : string.Format(this.WordWrap(BaseDescriptions.ITEM_STILL_LOCKED, this.ConsoleWidth), (object) item.Name));
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemUnlocked(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ITEM_UNLOCKED, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemNotLockAble(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ITEM_NOT_LOCKABLE, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemOpen(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.NOW_OPEN, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemDropSuccess(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ITEM_DROP, this.ConsoleWidth), (object) item.AccusativeArticleName.LowerFirstChar());
      Console.WriteLine();
      return true;
    }

    public bool ItemDropSuccess(AHereticObject itemToDrop, AHereticObject containerItem)
    {
      Console.Write(containerItem.IsSurfaceContainer ? this.WordWrap(BaseDescriptions.ITEM_DROP_ONTO, this.ConsoleWidth) : this.WordWrap(BaseDescriptions.ITEM_DROP_INTO, this.ConsoleWidth), (object) itemToDrop.AccusativeArticleName.LowerFirstChar(), (object) containerItem.AccusativeArticleName.LowerFirstChar());
      Console.WriteLine();
      return true;
    }

    public bool ItemIsNotAContainer(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.ITEM_NOT_A_CONTAINER, this.ConsoleWidth), (object) item.Name);
      Console.WriteLine();
      return true;
    }

    public virtual bool ItemNotOwned() => this.Resource(BaseDescriptions.ITEM_NOT_OWNED);

    public virtual bool ItemAlreadyOwned() => this.Resource(BaseDescriptions.ITEM_ALREADY_OWNED);

    public virtual bool DestinationNode(
      Location activeLocation,
      IDictionary<Location, IEnumerable<Heretic.InteractiveFiction.Objects.DestinationNode>> locationMap)
    {
      if (locationMap.ContainsKey(activeLocation))
      {
        List<Heretic.InteractiveFiction.Objects.DestinationNode> list = locationMap[activeLocation].Where<Heretic.InteractiveFiction.Objects.DestinationNode>((Func<Heretic.InteractiveFiction.Objects.DestinationNode, bool>) (l => !l.IsHidden)).ToList<Heretic.InteractiveFiction.Objects.DestinationNode>();
        if (list.Any<Heretic.InteractiveFiction.Objects.DestinationNode>())
        {
          foreach (Heretic.InteractiveFiction.Objects.DestinationNode message in list)
          {
            if (message.ShowInDescription)
              Console.Write(this.WordWrap((object) message, this.ConsoleWidth));
          }
          Console.WriteLine();
        }
      }
      return true;
    }

    public virtual bool Misconcept() => this.Resource(BaseDescriptions.MISCONCEPTION);

    public virtual bool NothingToTake() => this.Resource(BaseDescriptions.NOTHING_TO_TAKE);

    public virtual bool NoAnswer(string phrase)
    {
      Console.WriteLine(string.Format(this.WordWrap(BaseDescriptions.NO_ANSWER, this.ConsoleWidth), (object) phrase) ?? "");
      return true;
    }

    public virtual bool NoAnswerToInvisibleObject(Character character)
    {
      Genders gender = character.Grammar.Gender;
      if (true)
        ;
      string str1;
      switch (gender)
      {
        case Genders.Female:
          str1 = BaseDescriptions.GENDER_FEMALE;
          break;
        case Genders.Male:
          str1 = BaseDescriptions.GENDER_MALE;
          break;
        default:
          str1 = BaseDescriptions.GENDER_UNKNOWN;
          break;
      }
      if (true)
        ;
      string str2 = str1;
      Console.WriteLine(string.Format(this.WordWrap(BaseDescriptions.ASK_FOR_INVISIBLE_OBJECT, this.ConsoleWidth), (object) str2) ?? "");
      return true;
    }

    public virtual bool NoAnswerToQuestion(string phrase)
    {
      Console.WriteLine(string.Format(this.WordWrap(BaseDescriptions.NO_ANSWER_TO_QUESTION, this.ConsoleWidth), (object) phrase.LowerFirstChar()) ?? "");
      return true;
    }

    public abstract bool Opening();

    public abstract bool Closing();

    public virtual bool Resource(string resource)
    {
      if (!string.IsNullOrEmpty(resource))
      {
        Console.Write(this.WordWrap(resource, this.ConsoleWidth));
        Console.WriteLine();
      }
      return true;
    }

    public bool FormattedResource(string resource, string text, bool lowerFirstLetter = false)
    {
      if (!string.IsNullOrEmpty(resource) && !string.IsNullOrEmpty(text))
      {
        Console.Write(this.WordWrap(string.Format(resource, lowerFirstLetter ? (object) text.LowerFirstChar() : (object) text), this.ConsoleWidth));
        Console.WriteLine();
      }
      return true;
    }

    public virtual bool Score(int score, int maxScore)
    {
      Console.WriteLine(string.Format(BaseDescriptions.SCORE, (object) score, (object) maxScore) ?? "");
      return true;
    }

    public virtual bool Talk(Character character)
    {
      if (character != null)
      {
        string message = character.DoTalk();
        if (!string.IsNullOrEmpty(message))
          Console.WriteLine(this.WordWrap(message, this.ConsoleWidth));
        else
          Console.WriteLine(this.WordWrap(BaseDescriptions.WHAT, this.ConsoleWidth));
        Console.WriteLine();
      }
      return true;
    }

    public abstract bool TitleAndScore(int score, int maxScore);

    public bool ToolNotVisible() => this.Resource(BaseDescriptions.TOOL_NOT_VISIBLE);

    public virtual bool WrongKey(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.WRONG_KEY, this.ConsoleWidth), (object) item.Name.LowerFirstChar());
      Console.WriteLine();
      return true;
    }

    public virtual bool Prompt()
    {
      Console.Write("> ");
      return true;
    }

    public virtual bool PayWithWhat() => this.Resource(BaseDescriptions.PAY_WITH_WHAT);

    public virtual bool SameActionAgain() => this.Resource(BaseDescriptions.SAME_ACTION_AGAIN);

    public virtual bool NoEvent() => this.Resource(BaseDescriptions.NO_EVENT);

    public virtual bool WayIsLocked(AHereticObject item) => !string.IsNullOrEmpty((string) item.LockDescription) ? this.Resource((string) item.LockDescription) : this.Resource(BaseDescriptions.WAY_IS_LOCKED);

    public virtual bool WayIsClosed(AHereticObject item) => !string.IsNullOrEmpty((string) item.CloseDescription) ? this.Resource((string) item.CloseDescription) : this.Resource(BaseDescriptions.WAY_IS_CLOSED);

    public virtual bool ImpossibleUnlock(AHereticObject item)
    {
      Console.Write(this.WordWrap(BaseDescriptions.IMPOSSIBLE_UNLOCK, this.ConsoleWidth), (object) item.AccusativeArticleName.LowerFirstChar());
      Console.WriteLine();
      return true;
    }

    protected virtual string WordWrap(string message, int width)
    {
      string[] strArray = message.Split(Environment.NewLine);
      StringBuilder stringBuilder = new StringBuilder();
      foreach (string str1 in strArray)
      {
        string str2;
        int length;
        for (str2 = str1; !string.IsNullOrWhiteSpace(str2) && str2.Length > width; str2 = str2.Substring(length + 1))
        {
          int num = 0;
          do
          {
            length = num;
            num = str2.IndexOf(' ', length + 1);
          }
          while (num > -1 && num < width);
          stringBuilder.AppendLine(str2.Substring(0, length));
        }
        stringBuilder.AppendLine(str2);
      }
      return stringBuilder.ToString();
    }

    protected virtual string WordWrap(object message, int width) => this.WordWrap(message.ToString(), width);

    private string GetObjectName(string itemName) => itemName.Split('|')[0].Trim();

    private string GetRandomPhrase(string message)
    {
      string[] strArray = message.Split("|");
      int index = new Random().Next(strArray.Length);
      return strArray[index];
    }

    
    private string WordWrap(string message)
      => base.WordWrap(message, ConsoleWidth);

    public override bool Opening()
      => throw new NotImplementedException();

    public override bool Closing()
      => throw new NotImplementedException();

    public override bool TitleAndScore(int score, int maxScore)
      => throw new NotImplementedException();

    public override bool Credits()
      => throw new NotImplementedException();
  }