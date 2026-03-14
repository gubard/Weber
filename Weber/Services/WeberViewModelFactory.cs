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
    public WeberViewModelFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public FilesViewModel CreateFiles(
        AvaloniaList<FileObjectNotify> files,
        FileObjectNotify selectedFile
    )
    {
        return new(
            files,
            selectedFile,
            _serviceProvider.GetService<IFileStorageUiService>(),
            _serviceProvider.GetService<Application>(),
            _serviceProvider.GetService<IAppResourceService>(),
            _serviceProvider.GetService<IStringFormater>(),
            _serviceProvider.GetService<ISafeExecuteWrapper>()
        );
    }

    private readonly IServiceProvider _serviceProvider;
}
