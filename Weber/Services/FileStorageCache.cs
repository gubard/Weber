using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Avalonia.Collections;
using Avalonia.Threading;
using Gaia.Helpers;
using Gaia.Services;
using Inanna.Helpers;
using Inanna.Services;
using Neotoma.Contract.Models;
using Neotoma.Contract.Services;
using Weber.Models;

namespace Weber.Services;

public interface IFileStorageMemoryCache : IMemoryCache<NeotomaPostRequest, NeotomaGetResponse>
{
    AvaloniaList<FileObjectNotify> GetFiles(string dir);
}

public interface IFileStorageUiCache
    : IUiCache<NeotomaPostRequest, NeotomaGetResponse, IFileStorageMemoryCache>
{
    AvaloniaList<FileObjectNotify> GetFiles(string dir);
}

public sealed class FileStorageUiCache
    : UiCache<NeotomaPostRequest, NeotomaGetResponse, IFileStorageDbCache, IFileStorageMemoryCache>,
        IFileStorageUiCache
{
    public FileStorageUiCache(IFileStorageDbCache dbCache, IFileStorageMemoryCache memoryCache)
        : base(dbCache, memoryCache) { }

    public AvaloniaList<FileObjectNotify> GetFiles(string dir)
    {
        return MemoryCache.GetFiles(dir);
    }
}

public sealed class FileStorageMemoryCache
    : MemoryCache<FileObjectNotify, NeotomaPostRequest, NeotomaGetResponse>,
        IFileStorageMemoryCache
{
    public FileStorageMemoryCache(IServiceProvider serviceProvider)
        : base(serviceProvider) { }

    public override ConfiguredValueTaskAwaitable UpdateAsync(
        NeotomaPostRequest source,
        CancellationToken ct
    )
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var item in source.Creates)
            {
                foreach (var value in item.Value)
                {
                    UpdateItem(value, item.Key);
                }
            }

            foreach (var id in source.Deletes)
            {
                var item = GetItem(id);
                Items.Remove(id);

                if (_files.TryGetValue(item.Dir, out var files))
                {
                    files.Remove(item);
                }
            }
        });

        return TaskHelper.ConfiguredCompletedTask;
    }

    public override ConfiguredValueTaskAwaitable UpdateAsync(
        NeotomaGetResponse source,
        CancellationToken ct
    )
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var item in source.All)
            {
                var files = GetFiles(item.Key);

                var values = item
                    .Value.OrderBy(x => x.Name)
                    .Select(x => UpdateItem(x, item.Key))
                    .ToArray();

                files.UpdateOrder(values);
            }

            foreach (var item in source.Info)
            {
                var files = GetFiles(item.Key);

                var values = item
                    .Value.OrderBy(x => x.Name)
                    .Select(x => UpdateItem(x, item.Key))
                    .ToArray();

                files.UpdateOrder(values);
            }

            foreach (var data in source.Data)
            {
                UpdateItem(data);
            }
        });

        return TaskHelper.ConfiguredCompletedTask;
    }

    public AvaloniaList<FileObjectNotify> GetFiles(string dir)
    {
        if (_files.TryGetValue(dir, out var files))
        {
            return files;
        }

        files = new();
        _files[dir] = files;

        return files;
    }

    private readonly Dictionary<string, AvaloniaList<FileObjectNotify>> _files = new();

    private FileObjectNotify UpdateItem(FileObjectInfo value, string dir)
    {
        var item = GetItem(value.Id);
        item.Name = value.Name;
        item.Description = value.Description;
        item.Dir = dir;

        item.Status =
            value.Hash == item.Hash
                ? FileObjectNotifyStatus.Updated
                : FileObjectNotifyStatus.WrongHash;

        return item;
    }

    private FileObjectNotify UpdateItem(FileObject value, string dir)
    {
        var item = GetItem(value.Id);
        item.Name = value.Name;
        item.Description = value.Description;
        item.Data = value.Data;
        item.Dir = dir;
        var files = GetFiles(dir);
        files.AddSorted(item, x => x.Name);

        return item;
    }

    private FileObjectNotify UpdateItem(FileObjectData value)
    {
        var item = GetItem(value.Id);
        item.Data = value.Data;
        item.Hash = value.Hash;
        item.Status = FileObjectNotifyStatus.Updated;

        return item;
    }
}
