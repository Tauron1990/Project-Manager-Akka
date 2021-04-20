﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Tauron.Application.Files.Ini.Parser
{
    [PublicAPI]
    public sealed class IniParser
    {
        private static readonly char[] KeyValueChar = {'='};

        private readonly TextReader _reader;

        public IniParser(TextReader reader) => _reader = Argument.NotNull(reader, nameof(reader));

        public IniFile Parse()
        {
            var entrys = new Dictionary<string, GroupDictionary<string, string>>();
            var currentSection = new GroupDictionary<string, string>();
            string? currentSectionName = null;

            foreach (var line in _reader.EnumerateTextLines())
            {
                if (line[0] == '[' && line[^1] == ']')
                {
                    if (currentSectionName != null) entrys[currentSectionName] = currentSection;

                    currentSectionName = line.Trim().Trim('[', ']');
                    currentSection = new GroupDictionary<string, string>();
                    continue;
                }

                var content = line.Split(KeyValueChar, 2, StringSplitOptions.RemoveEmptyEntries);
                if (content.Length <= 1)
                    continue;
                currentSection[content[0]].Add(content[1]);
            }

            if (currentSectionName != null)
                entrys[currentSectionName] = currentSection;

            var sections = ImmutableDictionary<string, IniSection>.Empty;

            foreach (var (key, value) in entrys)
            {
                var entries = ImmutableDictionary<string, IniEntry>.Empty;

                foreach (var (entryKey, collection) in value) 
                    entries = collection.Count < 1 
                        ? entries.Add(entryKey, new ListIniEntry(entryKey, ImmutableList<string>.Empty.AddRange(collection))) 
                        : entries.Add(entryKey, new SingleIniEntry(entryKey, collection.ElementAt(0)));

                sections = sections.Add(key, new IniSection(key, entries));
            }

            return new IniFile(sections);
        }
    }
}