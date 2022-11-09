namespace SimpleProjectManager.Server.Data.Data;

public sealed record DbFileInfoData
{
    public string Id { get; init; } = string.Empty;

    public string ProjectName { get; init; } = string.Empty;

    public string FileName { get; init; } = string.Empty;

    public long Size { get; init; }

    public int FileType { get; init; }

    public string Mime { get; init; } = string.Empty;

    /*public static void Configurate(MapperConfigurationExpression mapperConfiguration)
    {
        mapperConfiguration.CreateMap<FileInfoData, DbFileInfoData>()
           .ForMember(data => data.Id, exp => exp.MapFrom(fid => fid.Id.Value))
           .ForMember(data => data.ProjectName, exp => exp.MapFrom(fid => fid.ProjectName.Value))
           .ForMember(data => data.FileName, exp => exp.MapFrom(fid => fid.FileName.Value))
           .ForMember(data => data.Size, exp => exp.MapFrom(fid => fid.Size.Value))
           .ForMember(data => data.FileType, exp => exp.MapFrom(fid => (int)fid.FileType))
           .ForMember(data => data.FileMime, exp => exp.MapFrom(fid => fid.Mime.Value));

        mapperConfiguration.CreateMap<DbFileInfoData, FileInfoData>()
           .ForMember(data => data.Id, exp => exp.MapFrom(fid => new ProjectFileId(fid.Id)))
           .ForMember(data => data.ProjectName, exp => exp.MapFrom(fid => new ProjectName(fid.ProjectName)))
           .ForMember(data => data.FileName, exp => exp.MapFrom(fid => new FileName(fid.FileName)))
           .ForMember(data => data.Size, exp => exp.MapFrom(fid => new FileSize(fid.Size)));
    }*/
}