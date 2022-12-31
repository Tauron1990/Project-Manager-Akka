using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Tauron.Application.SoftwareRepo.Data;
using Tauron.Application.SoftwareRepo.Mutation;
using Tauron.Application.VirtualFiles;
using Tauron.Application.Workshop;
using Tauron.Application.Workshop.Driver;
using Tauron.Application.Workshop.Mutating;
using Tauron.Application.Workshop.Mutation;

namespace Tauron.Application.SoftwareRepo;

[PublicAPI]
public sealed class SoftwareRepository : Workspace<SoftwareRepository, ApplicationList>
{
    internal const string FileName = "Apps.json";

    public SoftwareRepository(IDriverFactory factory, IDirectory path)
        : base(factory)
    {
        Path = path;
        Changed = Engine.EventSource(
            mc => mc.GetChange<CommonChange>().ApplicationList,
            context => context.Change is CommonChange);
        Changed.RespondOn(Save);
    }

    public IDirectory Path { get; }

    public ApplicationList ApplicationList { get; private set; } =
        new(string.Empty, string.Empty, ImmutableList<ApplicationEntry>.Empty);

    public IEventSource<ApplicationList> Changed { get; }


    internal void Init()
    {
        IFile file = GetFile();

        if(!file.Exist)
            throw new InvalidOperationException("Apps File not found");

        using var reader = new StreamReader(file.Open(FileAccess.Read));
        Reset(JsonConvert.DeserializeObject<ApplicationList>(reader.ReadToEnd()) ?? throw new InvalidOperationException("Deserilizing File Failed"));
    }

    internal void InitNew()
    {
        IFile file = GetFile();

        if(!file.Exist)
            file.Delete();
        using var writer = new StreamWriter(file.CreateNew());
        writer.Write(JsonConvert.SerializeObject(ApplicationList));
    }

    private IFile GetFile() => Path.GetFile(FileName);

    private void Save(ApplicationList al)
    {
        using var writer = new StreamWriter(GetFile().CreateNew());
        writer.Write(JsonConvert.SerializeObject(al));
    }

    public void ChangeName(string? name = null, string? description = null)
        => Engine.Mutate(
            nameof(ChangeName),
            mco => mco.Select(
                mc =>
                {
                    ApplicationList? newData = null;
                    if(!string.IsNullOrWhiteSpace(name))
                        newData = mc.Data with { Name = name };
                    if(!string.IsNullOrWhiteSpace(description))
                        newData ??= mc.Data with { Description = description };

                    return newData is null ? mc : mc.Update(new CommonChange(newData), newData);
                }));

    public long Get(string name) => ApplicationList.ApplicationEntries.Find(ae => string.Equals(ae.Name, name, StringComparison.Ordinal))?.Id ?? -1;

    public void AddApplication(
        string name, long id, string url, Version version, string originalRepository,
        string brnachName)
    {
        if(Get(name) != -1)
            return;

        Engine.Mutate(
            nameof(AddApplication),
            mco => mco.Select(
                mc =>
                {
                    ApplicationList newData = mc.Data with
                                              {
                                                  ApplicationEntries =
                                                  mc.Data.ApplicationEntries.Add(
                                                      new ApplicationEntry(
                                                          name,
                                                          version,
                                                          id,
                                                          ImmutableList<DownloadEntry>.Empty.Add(new DownloadEntry(version, url)),
                                                          originalRepository,
                                                          brnachName)),
                                              };

                    return mc.Update(new CommonChange(newData), newData);
                }));
    }

    public void UpdateApplication(long id, Version version, string url)
        => Engine.Mutate(
            nameof(UpdateApplication),
            mco => mco.Select(
                mc =>
                {
                    ApplicationEntry? entry = ApplicationList.ApplicationEntries.Find(ae => ae.Id == id);

                    if(entry is null)
                        return mc;

                    ApplicationList newData = mc.Data with
                                              {
                                                  ApplicationEntries =
                                                  mc.Data.ApplicationEntries.Replace(
                                                      entry,
                                                      entry with
                                                      {
                                                          Last = version,
                                                          Downloads = entry.Downloads.Add(new DownloadEntry(version, url)),
                                                      }),
                                              };

                    return mc.Update(new CommonChange(newData), newData);
                }));

    public void Save() => Save(ApplicationList);

    protected override MutatingContext<ApplicationList> GetDataInternal()
        => MutatingContext<ApplicationList>.New(ApplicationList);

    protected override void SetDataInternal(MutatingContext<ApplicationList> data)
        => ApplicationList = data.Data;
}