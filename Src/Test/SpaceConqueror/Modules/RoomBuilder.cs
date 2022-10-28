namespace SpaceConqueror.Modules;

public class RoomBuilder
{
    internal Type Rule { get; set; }

    public RoomBuilder(Type rule)
        => Rule = rule;
}