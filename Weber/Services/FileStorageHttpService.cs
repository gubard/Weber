using System;
using System.Net.Http;
using System.Text.Json;
using Gaia.Models;
using Gaia.Services;
using Neotoma.Contract.Models;
using Neotoma.Contract.Services;

namespace Weber.Services;

public sealed class FileStorageHttpService(
    IFactory<HttpClient> httpClientFactory,
    JsonSerializerOptions options,
    ITryPolicyService tryPolicyService,
    IFactory<Memory<HttpHeader>> headersFactory
)
    : HttpService<NeotomaGetRequest, NeotomaPostRequest, NeotomaGetResponse, NeotomaPostResponse>(
        httpClientFactory,
        options,
        tryPolicyService,
        headersFactory
    ),
        IFileStorageHttpService
{
    protected override NeotomaGetRequest CreateHealthCheckGetRequest()
    {
        return new();
    }
}
