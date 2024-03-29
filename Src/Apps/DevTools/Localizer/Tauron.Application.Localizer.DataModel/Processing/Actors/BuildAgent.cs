﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Akka.Actor;
using Tauron.Application.Localizer.DataModel.Processing.Messages;
using Tauron.Localization;
using Tauron.TAkka;

namespace Tauron.Application.Localizer.DataModel.Processing.Actors
{
    public sealed class BuildAgent : ObservableActor
    {
        public BuildAgent()
        {
            Receive<PreparedBuild>(obs => obs.Select(OnBuild).ForwardToParent());
        }

        private static AgentCompled OnBuild(PreparedBuild build)
        {
            try
            {
                var agentName = Context.Self.Path.Name + ": ";

                Context.Sender.Tell(BuildMessage.GatherData(build.Operation, agentName));
                var data = GetData(build);

                Context.Sender.Tell(BuildMessage.GenerateLangFiles(build.Operation, agentName));
                GenerateJson(data, build.TargetPath);

                Context.Sender.Tell(BuildMessage.GenerateCsFiles(build.Operation, agentName));
                GenerateCode(data, build.TargetPath);

                Context.Sender.Tell(BuildMessage.AgentCompled(build.Operation, agentName));

                return new AgentCompled(Failed: false, Cause: null, build.Operation);
            }
            catch (Exception e)
            {
                return new AgentCompled(Failed: true, e, build.Operation);
            }
        }

        private static Dictionary<string, GroupDictionary<string, (string Id, string Content)>> GetData(
            PreparedBuild build)
        {
            var files = new Dictionary<string, GroupDictionary<string, (string Id, string Content)>>(StringComparer.Ordinal);

            var imports = new List<(string FileName, Project Project)>
                          {
                              (string.Empty, build.TargetProject)
                          };
            imports.AddRange(
                build.TargetProject.Imports
                   .Select(pn => build.ProjectFile.Projects.Find(p => string.Equals(p.ProjectName, pn, StringComparison.Ordinal)))
                   .Where(p => p != null)
                   .Select(p => build.BuildInfo.IntigrateProjects ? (string.Empty, p!) : (p!.ProjectName, p)));

            foreach ((string fileName, Project project) in imports)
            {
                if (!files.TryGetValue(fileName, out var entrys))
                {
                    entrys = new GroupDictionary<string, (string Id, string Content)>();
                    files[fileName] = entrys;
                }

                foreach ((string _, string id, var values) in project.Entries)
                    foreach (((string shortcut, string _), string value) in values)
                        entrys.Add(shortcut, (id, value));
            }

            return files;
        }

        private static void GenerateJson(
            Dictionary<string, GroupDictionary<string, (string Id, string Content)>> data,
            string targetPath)
        {
            if (!Directory.Exists(targetPath))
                Directory.CreateDirectory(targetPath);

            foreach ((string key, var value) in data)
                foreach ((string lang, var entrys) in value)
                    WriteEntry(targetPath, key, lang, entrys);
        }

        private static void WriteEntry(string targetPath, string key, string lang, ICollection<(string Id, string Content)> entrys)
        {

            var fileName = (string.IsNullOrWhiteSpace(key) ? string.Empty : key + ".") + lang + ".json";
            using var writer = new StreamWriter(File.Open(Path.Combine(targetPath, fileName), FileMode.Create));
            var tester = new HashSet<string>(StringComparer.Ordinal);
            writer.WriteLine("{");

            foreach ((string id, string content) in entrys)
                if(tester.Add(id))
                    writer.WriteLine($"  \"{id}\": \"{EscapeHelper.Ecode(content)}\",");

            writer.WriteLine("}");
            writer.Flush();
        }

        private static void GenerateCode(
            Dictionary<string, GroupDictionary<string, (string Id, string Content)>> data,
            string targetPath)
        {
            var ids = new HashSet<string>(
                data
                   .SelectMany(e => e.Value)
                   .SelectMany(e => e.Value)
                   .Select(e => e.Id), StringComparer.Ordinal);

            var entrys = new GroupDictionary<string, (string FieldName, string Original)>
                         {
                             string.Empty
                         };

            foreach (var id in ids)
            {
                var result = id.Split('_', 2, StringSplitOptions.RemoveEmptyEntries);
                if (result.Length == 1)
                    entrys.Add(string.Empty, (result[0].Replace("_", "", StringComparison.Ordinal), id));
                else
                    entrys.Add(result[0], (result[1].Replace("_", "", StringComparison.Ordinal), id));
            }

            using var file = new StreamWriter(File.Open(Path.Combine(targetPath, "LocLocalizer.cs"), FileMode.Create));

            WriteHeader(file);

            var classes = new List<string>();

            foreach ((string key, var value) in entrys)
            {
                if (string.IsNullOrWhiteSpace(key)) continue;

                classes.Add(key);

                file.WriteLine($"\t\tpublic sealed class {key}Res");
                file.WriteLine("\t\t{");
                WriteClassData(file, key + "Res", value, 3);
                file.WriteLine("\t\t}");
            }

            WriteClassData(
                file,
                "LocLocalizer",
                entrys[string.Empty],
                2,
                writer =>
                {
                    foreach (var @class in classes)
                        writer(@class + " = new " + @class + "Res" + "(system);");
                });

            foreach (var @class in classes)
                file.WriteLine("\t\tpublic " + @class + "Res " + @class + " { get; }");

            file.WriteLine("\t\tprivate static Task<string> ToString(Task<Option<object>> task)");
            file.WriteLine("\t\t\t=> task.ContinueWith(t => t.Result.Select(o => o as string ?? string.Empty).GetOrElse(string.Empty));");

            file.WriteLine("\t}");
            file.WriteLine("}");

            file.Flush();
        }

        private static void WriteHeader(StreamWriter file)
        {

            file.WriteLine("using System.CodeDom.Compiler;");
            file.WriteLine("using System.Threading.Tasks;");
            file.WriteLine("using Akka.Actor;");
            file.WriteLine("using JetBrains.Annotations;");
            file.WriteLine("using Tauron.Localization;");
            file.WriteLine("using Akka.Util;");
            file.WriteLine();
            file.WriteLine("namespace Tauron.Application.Localizer.Generated");
            file.WriteLine("{");
            file.WriteLine("\t[PublicAPI, GeneratedCode(\"Localizer\", \"1\")]");
            file.WriteLine("\tpublic sealed partial class LocLocalizer");
            file.WriteLine("\t{");
        }

        private static void WriteClassData(
            StreamWriter writer, string className,
            ICollection<(string FieldName, string Original)> entryValue, int tabs,
            Action<Action<string>>? constructorCallback = null)
        {
            foreach ((string fieldName, var _) in entryValue)
                writer.WriteLine(
                    $"{new string('\t', tabs)}private readonly Task<string> {ToRealFieldName(fieldName)};");

            writer.WriteLine($"{new string('\t', tabs)}public {className}(ActorSystem system)");
            writer.WriteLine(new string('\t', tabs) + "{");
            tabs++;
            writer.WriteLine($"{new string('\t', tabs)}var loc = system.Loc();");

            // ReSharper disable once AccessToModifiedClosure
            constructorCallback?.Invoke(s => writer.WriteLine($"{new string('\t', tabs)} {s}"));

            foreach ((string fieldName, string original) in entryValue)
                writer.WriteLine(
                    $"{new string('\t', tabs)}{ToRealFieldName(fieldName)} = LocLocalizer.ToString(loc.RequestTask(\"{original}\"));");

            tabs--;
            writer.WriteLine(new string('\t', tabs) + "}");

            foreach ((string fieldName, string _) in entryValue)
                writer.WriteLine(
                    $"{new string('\t', tabs)}public string {fieldName} => {ToRealFieldName(fieldName)}.Result;");
        }

        private static string ToRealFieldName(string name) => "__" + name;
    }
}