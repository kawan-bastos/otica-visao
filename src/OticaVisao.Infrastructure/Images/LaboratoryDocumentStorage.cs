using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace OticaVisao.Infrastructure.Images;

public sealed class LaboratoryDocumentStorageOptions { public const string SectionName = "LaboratoryDocumentStorage"; public string? Path { get; set; } }
public interface ILaboratoryDocumentStorage
{
    Task<string> SaveAsync(Stream content, string extension, CancellationToken cancellationToken = default);
    Task<StoredFrameImage?> OpenReadAsync(string fileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string fileName, CancellationToken cancellationToken = default);
}
internal sealed class LocalLaboratoryDocumentStorage(IWebHostEnvironment environment, IOptions<LaboratoryDocumentStorageOptions> options) : ILaboratoryDocumentStorage
{
    private readonly string path = System.IO.Path.GetFullPath(string.IsNullOrWhiteSpace(options.Value.Path) ? System.IO.Path.Combine(environment.ContentRootPath,"App_Data","laboratory-documents") : (System.IO.Path.IsPathRooted(options.Value.Path) ? options.Value.Path : System.IO.Path.Combine(environment.ContentRootPath,options.Value.Path)));
    private static readonly Dictionary<string,string> Types = new(StringComparer.OrdinalIgnoreCase) { [".jpg"]="image/jpeg",[".jpeg"]="image/jpeg",[".png"]="image/png",[".webp"]="image/webp" };
    public async Task<string> SaveAsync(Stream content,string extension,CancellationToken token=default) { extension=extension.ToLowerInvariant(); if(!Types.ContainsKey(extension))throw new ArgumentException("Formato de imagem não permitido."); Directory.CreateDirectory(path); var name=$"{Guid.NewGuid():N}{extension}"; await using var target=new FileStream(System.IO.Path.Combine(path,name),FileMode.CreateNew,FileAccess.Write,FileShare.None); await content.CopyToAsync(target,token); return name; }
    public Task<StoredFrameImage?> OpenReadAsync(string name,CancellationToken token=default) { if(!Safe(name)||!Types.TryGetValue(System.IO.Path.GetExtension(name),out var type))return Task.FromResult<StoredFrameImage?>(null); var file=System.IO.Path.Combine(path,name); return Task.FromResult<StoredFrameImage?>(File.Exists(file)?new(new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.Read),type):null); }
    public Task DeleteAsync(string name,CancellationToken token=default) { if(Safe(name)){var file=System.IO.Path.Combine(path,name);if(File.Exists(file))File.Delete(file);}return Task.CompletedTask; }
    private static bool Safe(string name)=>!string.IsNullOrWhiteSpace(name)&&name==System.IO.Path.GetFileName(name)&&name.Length<=80;
}
