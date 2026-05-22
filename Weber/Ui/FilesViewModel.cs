using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gaia.Helpers;
using Gaia.Services;
using Inanna.Helpers;
using Inanna.Models;
using Inanna.Services;
using Inanna.Ui;
using Weber.Models;
using Weber.Services;

namespace Weber.Ui;

public sealed partial class FilesViewModel : ViewModelBase
{
    public FilesViewModel(
        AvaloniaList<FileObjectNotify> files,
        FileObjectNotify selectedFile,
        IFileStorageUiService fileStorageUiService,
        Application app,
        IAppResourceService appResourceService,
        IStringFormater stringFormater,
        ISafeExecuteWrapper safeExecuteWrapper,
        IDialogService dialogService,
        ICommandFactory commandFactory
    )
        : base(safeExecuteWrapper)
    {
        _files = files;
        _selectedFile = selectedFile;
        _fileStorageUiService = fileStorageUiService;
        _app = app;
        _appResourceService = appResourceService;
        _stringFormater = stringFormater;
        _safeExecuteWrapper = safeExecuteWrapper;
        _dialogService = dialogService;
        _commandFactory = commandFactory;
    }

    [ObservableProperty]
    private FileObjectNotify _selectedFile;

    private readonly AvaloniaList<FileObjectNotify> _files;
    private readonly IFileStorageUiService _fileStorageUiService;
    private readonly Application _app;
    private readonly IAppResourceService _appResourceService;
    private readonly IStringFormater _stringFormater;
    private readonly IDialogService _dialogService;
    private readonly ISafeExecuteWrapper _safeExecuteWrapper;
    private readonly ICommandFactory _commandFactory;

    [RelayCommand]
    private void NextFile()
    {
        var index = _files.IndexOf(SelectedFile);

        if (index == _files.Count - 1)
        {
            return;
        }

        if (index == -1)
        {
            return;
        }

        WrapCommand(() => Dispatcher.UIThread.Post(() => SelectedFile = _files[index + 1]));
    }

    [RelayCommand]
    private void PreviousFile()
    {
        var index = _files.IndexOf(SelectedFile);

        if (index == 0)
        {
            return;
        }

        if (index == -1)
        {
            return;
        }

        WrapCommand(() => Dispatcher.UIThread.Post(() => SelectedFile = _files[index - 1]));
    }

    [RelayCommand]
    private async Task DeleteFileAsync(CancellationToken ct)
    {
        await WrapCommandAsync(
            () =>
            {
                var header = _appResourceService
                    .GetResource<string>("Lang.Delete")
                    .ToDialogHeader();

                var content = _stringFormater.Format(
                    _appResourceService.GetResource<string>("Lang.AskDelete"),
                    SelectedFile.Name
                );

                var buttonContent = _appResourceService.GetResource<string>("Lang.Delete");

                var command = _commandFactory.CreateCommand(async c =>
                {
                    var id = SelectedFile.Id;
                    await _dialogService.CloseMessageBoxAsync(c);

                    if (_files.Count == 1)
                    {
                        await _dialogService.CloseMessageBoxAsync(c);
                    }
                    else
                    {
                        var index = _files.IndexOf(SelectedFile);
                        SelectedFile = index > 0 ? _files[index - 1] : _files[0];
                    }

                    return await _fileStorageUiService.PostAsync(
                        Guid.NewGuid(),
                        new() { Deletes = [id] },
                        c
                    );
                });

                var button = new DialogButton(
                    buttonContent,
                    command,
                    null,
                    DialogButtonType.Danger
                );

                var dialog = new DialogViewModel(
                    header,
                    content,
                    _safeExecuteWrapper,
                    button,
                    _dialogService.CancelButton
                );

                return _dialogService.ShowMessageBoxAsync(dialog, ct);
            },
            ct
        );
    }

    [RelayCommand]
    private async Task DownloadAsync(CancellationToken ct)
    {
        await WrapCommandAsync(
            async () =>
            {
                var file = await _app.GetTopLevel()
                    .ThrowIfNull()
                    .StorageProvider.SaveFilePickerAsync(
                        new()
                        {
                            Title = _stringFormater.Format(
                                _appResourceService.GetResource<string>("Lang.SaveItem"),
                                SelectedFile.Name
                            ),
                            SuggestedFileName = SelectedFile.Name,
                        }
                    );

                if (file is null)
                {
                    return;
                }

                await using var stream = await file.OpenWriteAsync();
                await stream.WriteAsync(SelectedFile.Data, ct);
            },
            ct
        );
    }
}
