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
        private AdxBannerAd _bannerAd;
        private bool _isLoading;

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        public AdxBackendAdsBanner(AdxBackendAds parent, ILogger logger)
        {
            _parent = parent;
            _logger = logger;
        }

        public async Task LoadBannerAsync(CancellationToken cancellationToken = default)
        {
            if (_isLoading)
            {
                _logger.Log("[ADX 광고] 배너 광고 이미 로딩 중입니다. 무시합니다.");
                return;
            }
            if (!_parent.IsInitialized)
            {
                _logger.LogError("[ADX 광고] ADX SDK가 초기화되지 않았습니다. 배너 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            _isLoading = true;
            
            _bannerAd?.Destroy();
            _bannerAd = new AdxBannerAd(_parent.adxBannerAdUnitId, AdxBannerAd.AD_SIZE_320x50, AdxBannerAd.POSITION_TOP);

            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded()
            {
                _logger.Log("[ADX 광고] 배너 광고 로드 완료");
                tcs.TrySetResult(true);
                _isLoading = false;
                OnAdRevenue?.Invoke(_parent.adxBannerAdUnitId, 0); // 실제 수익은 SDK에서 받아야 함
            }
            void OnFailed(int error)
            {
                _logger.LogError($"[ADX 광고] 배너 광고 로드 실패: {error}");
                OnAdError?.Invoke();
                tcs.TrySetException(new Exception(error.ToString()));
                _isLoading = false;
            }

            _bannerAd.OnAdLoaded -= OnLoaded;
            _bannerAd.OnAdFailedToLoad -= OnFailed;
            _bannerAd.OnAdLoaded += OnLoaded;
            _bannerAd.OnAdFailedToLoad += OnFailed;

            _bannerAd.Load();
            _logger.Log("[ADX 광고] 배너 광고 로드 요청");

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                try
                {
                    await tcs.Task;
                }
                catch (OperationCanceledException) // 또는 TaskCanceledException
                {
                    _logger.Log("[ADX 광고] 배너 광고 로드가 취소되었습니다.");
                    _isLoading = false;
                    // 필요하다면 OnAdError?.Invoke(); 호출 가능
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[ADX 광고] 배너 광고 로드 중 예외 발생: {ex.Message}");
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
            _logger.Log("[ADX 광고] 배너 광고 숨김 및 해제");
        }

        public void Dispose()
        {
            _bannerAd?.Destroy();
            _bannerAd = null;
        }
    }
}
