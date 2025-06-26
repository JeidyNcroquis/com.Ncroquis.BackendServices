using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Firebase.Analytics;
using VContainer;
using VContainer.Unity;
using R3;



namespace Ncroquis.Backend
{
    public class FirebaseBackendAnalytics : IBackendAnalytics, IAsyncStartable
    {
        [Inject] private readonly ILogger _logger;
        [Inject] private readonly FirebaseBackendProvider _provider;

        public ProviderKey providerKey => ProviderKey.FIREBASE;


        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {            
            await _provider.IsInitialized
                .Where(isInitialized => isInitialized)
                .FirstAsync(cancellation);

            _logger.Log($"[FIREBASE ANALYTICS] 분석 서비스를 활성화합니다.");
            
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        }


        public void LogEvent(string eventName, params AnalyticsParameter[] parameters)
        {
            if (!_provider.IsInitialized.CurrentValue)
            {
                _logger.Warning($"[{providerKey} ANALYTICS]: 초기화되지 않았습니다. 로그 전송 중단: {eventName}");
                return;
            }

            if (parameters == null || parameters.Length == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var firebaseParams = new List<Parameter>(parameters.Length);
            foreach (var param in parameters)
            {
                firebaseParams.Add(param.Value switch
                {
                    string s => new Parameter(param.Key, s),
                    long l => new Parameter(param.Key, l),
                    int i => new Parameter(param.Key, i),
                    double d => new Parameter(param.Key, d),
                    float f => new Parameter(param.Key, f),
                    bool b => new Parameter(param.Key, b ? 1L : 0L),
                    _ => new Parameter(param.Key, param.Value?.ToString() ?? string.Empty)
                });
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
        }

    }
}