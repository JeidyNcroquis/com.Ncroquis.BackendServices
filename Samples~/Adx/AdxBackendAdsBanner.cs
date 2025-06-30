using System;
using System.Threading;
using System.Threading.Tasks;
using AdxUnityPlugin;

namespace Ncroquis.Backend
{
    public class AdxBackendAdsBanner : IDisposable
    {
        private readonly AdxBackendAds _parent;
        private readonly ILogger _logger;
        private readonly string _adUnitId;
        private AdxBannerAd _bannerAd;
        private bool _isLoading;

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        public AdxBackendAdsBanner(AdxBackendAds parent, ILogger logger, string adUnitId)
        {
            _parent = parent;
            _logger = logger;
            _adUnitId = adUnitId;
        }

        public async Task LoadBannerAsync(BannerSize bannerSize = BannerSize.Size_320x50, BannerPosition bannerPosition = BannerPosition.Top, CancellationToken cancellationToken = default)
        {
            if (_isLoading)
            {
                _logger.Log("[ADX] 배너 광고 이미 로딩 중입니다. 무시합니다.");
                return;
            }
            if (!_parent.IsInitialized)
            {
                _logger.Error("[ADX] ADX SDK가 초기화되지 않았습니다. 배너 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            _isLoading = true;

            _bannerAd?.Destroy();
            _bannerAd = new AdxBannerAd(_adUnitId, (int)bannerSize, (int)bannerPosition);

            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded()
            {
                _logger.Log("[ADX] 배너 광고 로드 완료");
                tcs.TrySetResult(true);
                _isLoading = false;
                OnAdRevenue?.Invoke(_adUnitId, 0); // 실제 수익은 SDK에서 받아야 함
            }
            void OnFailed(int error)
            {
                _logger.Warning($"[ADX] 배너 광고 로드 실패: {error}");
                OnAdError?.Invoke();
                tcs.TrySetException(new Exception(error.ToString()));
                _isLoading = false;
            }

            _bannerAd.OnAdLoaded -= OnLoaded;
            _bannerAd.OnAdFailedToLoad -= OnFailed;
            _bannerAd.OnAdLoaded += OnLoaded;
            _bannerAd.OnAdFailedToLoad += OnFailed;

            _bannerAd.Load();
            _logger.Log("[ADX] 배너 광고 로드 요청");

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                try
                {
                    await tcs.Task;
                }
                catch (OperationCanceledException)
                {
                    _logger.Log("[ADX] 배너 광고 로드가 취소됨");
                    _isLoading = false;
                    return;
                }
                catch (Exception ex)
                {
                    _logger.Error($"[ADX] 배너 광고 로드 중 예외 발생: {ex.Message}");
                    OnAdError?.Invoke();
                    _isLoading = false;
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
