using Avalonia;
using Avalonia.Collections;
using Gaia.Services;
using Inanna.Services;
using Weber.Models;
using Weber.Ui;

namespace Weber.Services;

public interface IWeberViewModelFactory
{
    FilesViewModel CreateFiles(AvaloniaList<FileObjectNotify> files, FileObjectNotify selectedFile);
}

public sealed class WeberViewModelFactory : IWeberViewModelFactory
{
    public WeberViewModelFactory(
        IStringFormater stringFormater,
        IAppResourceService appResourceService,
        IFileStorageUiService fileStorageUiService,
        Application app
    )
    {
        _stringFormater = stringFormater;
        _appResourceService = appResourceService;
        _fileStorageUiService = fileStorageUiService;
        _app = app;
    }

    public FilesViewModel CreateFiles(
        AvaloniaList<FileObjectNotify> files,
        FileObjectNotify selectedFile
    )
    {
        return new(
            files,
            selectedFile,
            _fileStorageUiService,
            _app,
            _appResourceService,
            _stringFormater
        );
    }
    
    private readonly IStringFormater _stringFormater;
    private readonly IAppResourceService _appResourceService;
    private readonly IFileStorageUiService _fileStorageUiService;
    private readonly Application _app;
}
