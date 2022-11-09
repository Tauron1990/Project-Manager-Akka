using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using JetBrains.Annotations;
using Tauron.Application.Files.Ini.Parser;

namespace Tauron.Application.Files.Ini;

[PublicAPI]
[Serializable]
public record IniFile(ImmutableDictionary<string, IniSection> Sections) : IEnumerable<IniSection>
{
    public IniFile()
        : this(ImmutableDictionary<string, IniSection>.Empty) { }

    public IniSection? this[string name] => Sections.TryGetValue(name, out IniSection? section) ? section : null;

    public IEnumerator<IniSection> GetEnumerator() => Sections.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IniFile AddSection(string name)
    {
        var section = new IniSection(name);

        return this with { Sections = Sections.Add(name, section) };
    }

    public void Save(string path) => new IniWriter(this, new StreamWriter(path)).Write();


    public string GetData(string name, string sectionName, string defaultValue)
    {
        SingleIniEntry? keyData = this[sectionName]?.GetSingleEntry(name);

        if(keyData == null) return string.Empty;

        return string.IsNullOrWhiteSpace(keyData.Value) ? defaultValue : keyData.Value;
    }

    public IniFile SetData(string sectionName, string name, string value)
    {
        IniSection? section = this[sectionName];

        if(section != null)
            return this with { Sections = Sections.SetItem(sectionName, section with { Entries = section.Entries.SetItem(name, new SingleIniEntry(name, value)) }) };

        return this with { Sections = Sections.Add(sectionName, new IniSection(sectionName, ImmutableDictionary<string, IniEntry>.Empty.Add(name, new SingleIniEntry(name, value)))) };
    }

    public IniFile SetData(string sectionName, string name, IEnumerable<string> value)
    {
        IniSection? section = this[sectionName];

        if(section != null)
            return this with { Sections = Sections.SetItem(sectionName, section with { Entries = section.Entries.SetItem(name, new ListIniEntry(name, value)) }) };

        return this with { Sections = Sections.Add(sectionName, new IniSection(sectionName, ImmutableDictionary<string, IniEntry>.Empty.Add(name, new ListIniEntry(name, value)))) };
    }

    #region Content Load

    public static IniFile Parse(TextReader reader)
    {
        using (reader)
        {
            return new IniParser(reader).Parse();
        }
    }

    public static IniFile ParseContent(string content) => Parse(new StringReader(content));

    public static IniFile ParseFile(string path) => Parse(new StreamReader(path));

    public static IniFile ParseStream(Stream stream) => Parse(new StreamReader(stream));

    #endregion
}