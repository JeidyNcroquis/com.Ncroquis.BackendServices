using System;
using System.Threading;
using System.Threading.Tasks;
using AdxUnityPlugin;
using Cysharp.Threading.Tasks;

namespace Ncroquis.Backend
{
    public class AdxBackendAdsBanner : IDisposable
    {
        private readonly AdxBackendAds _parent;
        private readonly ILogger _logger;
        private readonly string _adUnitId;
        private AdxBannerAd _bannerAd;

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        public AdxBackendAdsBanner(AdxBackendAds parent, ILogger logger, string adUnitId)
        {
            _parent = parent;
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public async Task LoadBannerAsync(BannerSize bannerSize, BannerPosition bannerPosition, CancellationToken cancellationToken = default)
        {
            if (!_parent.IsInitialized)
            {
                _logger.Error("[ADX] ADX SDK가 초기화되지 않았습니다. 배너 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            _bannerAd?.Destroy();
            _bannerAd = new AdxBannerAd(_adUnitId, (int)bannerSize, (int)bannerPosition);

            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded()
            {
                UniTask.Post(() =>
                {
                    _logger.Log("[ADX] 배너 광고 로드 완료");
                    OnAdRevenue?.Invoke(_adUnitId, 0); // 실제 수익은 SDK에서 받아야 함
                    tcs.TrySetResult(true);
                });
            }

            void OnFailed(int error)
            {
                UniTask.Post(() =>
                {
                    _logger.Warning($"[ADX] 배너 광고 로드 실패: {error}");
                    OnAdError?.Invoke();
                    tcs.TrySetException(new Exception(error.ToString()));
                });
            }

            _bannerAd.OnAdLoaded -= OnLoaded;
            _bannerAd.OnAdFailedToLoad -= OnFailed;
            _bannerAd.OnAdLoaded += OnLoaded;
            _bannerAd.OnAdFailedToLoad += OnFailed;

            _bannerAd.Load();
            _logger.Log("[ADX] 배너 광고 로드 요청");

            using (cancellationToken.Register(() =>
            {
                UniTask.Post(() =>
                {
                    _logger.Log("[ADX] 배너 광고 로드가 취소됨");
                    tcs.TrySetCanceled();
                });
            }))
            {
                try
                {
                    await tcs.Task;
                }
                catch (OperationCanceledException)
                {
                    // 이미 처리됨
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ADX] 배너 광고 로드 중 예외 발생: {ex.Message}");
                    throw;
                }
            }
        }

        public void HideBannerAd()
        {
            _bannerAd?.Destroy();
            _bannerAd = null;
            _logger.Log("[ADX] 배너 광고 숨김 및 해제");
        }

        public void Dispose()
        {
            _bannerAd?.Destroy();
            _bannerAd = null;
        }
    }
}
