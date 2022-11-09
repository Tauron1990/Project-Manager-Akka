using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Tauron.Application.Files.Ini.Parser;

[PublicAPI]
public sealed class IniParser
{
    private static readonly char[] KeyValueChar = { '=' };

    private readonly TextReader _reader;

    public IniParser(TextReader reader) => _reader = reader;

    public IniFile Parse()
    {
        var entrys = new Dictionary<string, GroupDictionary<string, string>>();
        var currentSection = new GroupDictionary<string, string>();

        string? currentSectionName = _reader
           .EnumerateTextLines()
           .Aggregate<string?, string?>(null, (current, line) => ReadIniContent(line ?? string.Empty, current, entrys, ref currentSection));

        if(currentSectionName != null)
            entrys[currentSectionName] = currentSection;

        var sections = ImmutableDictionary<string, IniSection>.Empty;

        foreach ((string key, var value) in entrys)
            sections = CreateEntrys(value, sections, key);

        return new IniFile(sections);
    }

    private static ImmutableDictionary<string, IniSection> CreateEntrys(GroupDictionary<string, string> value, ImmutableDictionary<string, IniSection> sections, string key)
    {
        var entries = ImmutableDictionary<string, IniEntry>.Empty;

        foreach ((string entryKey, var collection) in value)
            entries = collection.Count < 1
                ? entries.Add(entryKey, new ListIniEntry(entryKey, ImmutableList<string>.Empty.AddRange(collection)))
                : entries.Add(entryKey, new SingleIniEntry(entryKey, collection.ElementAt(0)));

        sections = sections.Add(key, new IniSection(key, entries));

        return sections;
    }

    private static string? ReadIniContent(string line, string? currentSectionName, Dictionary<string, GroupDictionary<string, string>> entrys, ref GroupDictionary<string, string> currentSection)
    {
        if(line[0] == '[' && line[^1] == ']')
        {
            if(currentSectionName != null)
                entrys[currentSectionName] = currentSection;

            currentSectionName = line.Trim().Trim('[', ']');
            currentSection = new GroupDictionary<string, string>();

            return currentSectionName;
        }

        string[] content = line.Split(KeyValueChar, 2, StringSplitOptions.RemoveEmptyEntries);

        if(content.Length <= 1)
            return currentSectionName;

        currentSection[content[0]].Add(content[1]);

        return currentSectionName;
    }
}