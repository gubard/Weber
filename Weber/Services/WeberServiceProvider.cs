using Jab;

namespace Weber.Services;

[ServiceProviderModule]
[Singleton(typeof(IFileStorageMemoryCache), typeof(FileStorageMemoryCache))]
[Transient(typeof(IWeberViewModelFactory), typeof(WeberViewModelFactory))]
public interface IWeberServiceProvider;
