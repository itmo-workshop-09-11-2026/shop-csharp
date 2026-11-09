using System.IO.Compression;
using Microsoft.Extensions.FileProviders;

var site = Path.GetFullPath("site");
if (!File.Exists(Path.Combine(site, "index.html")))
    throw new InvalidOperationException("Запустите из корня проекта: нет site/index.html");
if (args.Contains("--pack"))
{
    var temporary = Path.GetFullPath($"shop-{Guid.NewGuid():N}.zip");
    try
    {
        var options = new EnumerationOptions { RecurseSubdirectories = true, AttributesToSkip = 0, IgnoreInaccessible = false };
        // Validate each level before descending so a link cannot lead outside site/.
        void CheckDirectory(string directory)
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
            {
                var attributes = File.GetAttributes(entry);
                if (attributes.HasFlag(FileAttributes.ReparsePoint))
                    throw new InvalidOperationException($"Ссылка в site: {entry}");
                if (attributes.HasFlag(FileAttributes.Directory)) CheckDirectory(entry);
            }
        }
        CheckDirectory(site);
        using (var zip = ZipFile.Open(temporary, ZipArchiveMode.Create))
            foreach (var file in Directory.EnumerateFiles(site, "*", options))
                zip.CreateEntryFromFile(file, Path.GetRelativePath(site, file).Replace('\\', '/'));
        File.Move(temporary, "shop.zip", overwrite: true);
    }
    finally { if (File.Exists(temporary)) File.Delete(temporary); }
    Console.WriteLine("Готово: shop.zip (index.html в корне)");
    return;
}
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls($"http://{Environment.GetEnvironmentVariable("HOST") ?? "127.0.0.1"}:{Environment.GetEnvironmentVariable("PORT") ?? "8000"}");
var app = builder.Build();
var files = new PhysicalFileProvider(site);
app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = files });
app.UseStaticFiles(new StaticFileOptions { FileProvider = files });
app.Run();
