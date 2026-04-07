using System.Threading;
using System.Threading.Tasks;
using Gaia.Services;
using Inanna.Services;
using Neotoma.Contract.Models;
using Neotoma.Contract.Services;

namespace Weber.Services;

public interface IFileStorageUiService
    : IUiService<NeotomaGetRequest, NeotomaPostRequest, NeotomaGetResponse, NeotomaPostResponse>;

public sealed class FileStorageUiService(
    IFileStorageHttpService httpService,
    IFileStorageDbService dbService,
    IFileStorageUiCache uiCache,
    INavigator navigator,
    string serviceName,
    IStatusBarService statusBarService,
    IInannaViewModelFactory factory
)
    : UiService<
        NeotomaGetRequest,
        NeotomaPostRequest,
        NeotomaGetResponse,
        NeotomaPostResponse,
        IFileStorageHttpService,
        IFileStorageDbService,
        IFileStorageUiCache
    >(httpService, dbService, uiCache, navigator, serviceName, statusBarService, factory),
        IFileStorageUiService
{
    protected override async ValueTask<IValidationErrors> RefreshServiceCore(CancellationToken ct)
    {
        var request = new NeotomaGetRequest { IsGetAll = true };
        var response = await DbService.GetAsync(request, ct);
        await UiCache.UpdateAsync(response, ct);

        return response;
    }
}
