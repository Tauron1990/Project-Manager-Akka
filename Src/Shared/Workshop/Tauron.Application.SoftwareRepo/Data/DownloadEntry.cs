using System;

namespace Tauron.Application.SoftwareRepo.Data;

public sealed record DownloadEntry(Version Version, string Url);