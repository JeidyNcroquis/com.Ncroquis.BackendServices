using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Ncroquis.Backend;


public class BackendInitializer : AProvidersInitializerBase
{
    private readonly BackendService _backendService;

    public BackendInitializer(IEnumerable<IBackendProvider> providers, BackendService backendService)
        : base(providers)
    {
        _backendService = backendService;
    }

    protected override async Task OnAfterAllInitialized(CancellationToken cancellation)
    {
        //Debug.Log($"[{_backendService.Provider().ProviderName}] 테스트");

        await Task.CompletedTask;
    }
}
