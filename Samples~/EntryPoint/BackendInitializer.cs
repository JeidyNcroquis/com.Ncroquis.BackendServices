using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Ncroquis.Backend;


public class BackendInitializer : AProvidersInitializerBase
{
    private readonly BackendService _backendService;
    private readonly ILogger _logger;

    public BackendInitializer(IEnumerable<IBackendProvider> providers, BackendService backendService, ILogger logger)
        : base(providers, logger)
    {
        _backendService = backendService;
        _logger = logger;
    }

    protected override async Task OnAfterAllInitialized(CancellationToken cancellation)
    {
        //_logger.Log($"[{_backendService.Provider().providerKey}] 테스트");      

        await Task.CompletedTask;
    }
}
