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
    string serviceName
)
    : UiService<
        NeotomaGetRequest,
        NeotomaPostRequest,
        NeotomaGetResponse,
        NeotomaPostResponse,
        IFileStorageHttpService,
        IFileStorageDbService,
        IFileStorageUiCache
    >(httpService, dbService, uiCache, navigator, serviceName),
        IFileStorageUiService
{
    protected override NeotomaGetRequest CreateGetRequestRefresh()
    {
        return new();
    }
}
