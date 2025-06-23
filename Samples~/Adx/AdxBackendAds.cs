using System;
using System.Threading;
using System.Threading.Tasks;
using VContainer;
using R3;

namespace Ncroquis.Backend
{
    public class AdxBackendAds : IBackendAds, IDisposable
    {
        public ProviderKey providerKey => ProviderKey.ADX;

#if UNITY_ANDROID || UNITY_EDITOR
        private readonly string adxBannerAdUnitId = "61ee2b7dcb8c67000100002a";
        private readonly string adxInterstitialAdUnitId = "61ee2e3fcb8c67000100002e";
        private readonly string adxRewardedAdUnitId = "61ee2e91cb8c67000100002f";
#elif UNITY_IPHONE
        private readonly string adxBannerAdUnitId = "6200fee42a918d0001000003";
        private readonly string adxInterstitialAdUnitId = "6200fef52a918d0001000007";
        private readonly string adxRewardedAdUnitId = "6200ff0c2a918d000100000d";
#endif

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        public AdxBackendAdsBanner Banner { get; }
        public AdxBackendAdsInterstitial Interstitial { get; }
        public AdxBackendAdsRewarded Rewarded { get; }

        private readonly AdxBackendProvider _adxProvider;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _cts = new();

        [Inject]
        public AdxBackendAds(AdxBackendProvider provider, ILogger logger)
        {
            _adxProvider = provider;
            _logger = logger;

            Banner = new AdxBackendAdsBanner(this, logger, adxBannerAdUnitId);
            Interstitial = new AdxBackendAdsInterstitial(this, logger, adxInterstitialAdUnitId);
            Rewarded = new AdxBackendAdsRewarded(this, logger, adxRewardedAdUnitId);

            // 이벤트 전달
            Banner.OnAdError += () => OnAdError?.Invoke();
            Interstitial.OnAdError += () => OnAdError?.Invoke();
            Rewarded.OnAdError += () => OnAdError?.Invoke();

            Banner.OnAdRevenue += (adUnitId, revenue) => OnAdRevenue?.Invoke(adUnitId, revenue);
            Interstitial.OnAdRevenue += (adUnitId, revenue) => OnAdRevenue?.Invoke(adUnitId, revenue);
            Rewarded.OnAdRevenue += (adUnitId, revenue) => OnAdRevenue?.Invoke(adUnitId, revenue);

            // 필요시 Provider 초기화 시 자동 로드
            _adxProvider.IsInitialized
                .Where(isInitialized => isInitialized)
                .Subscribe(async _ => await LoadAllAdsAsync(_cts.Token));
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            Banner?.Dispose();
            Interstitial?.Dispose();
            Rewarded?.Dispose();
            _logger.Log("[ADX] 광고 관리자 리소스 해제");
        }

        public async Task LoadAllAdsAsync(CancellationToken cancellationToken = default)
        {
            _logger.Log("[ADX] 모든 광고 로드 시작");
            await Task.WhenAll(
                LoadBannerAsync(BannerSize.Size_320x50, BannerPosition.Top, cancellationToken),
                LoadInterstitialAsync(cancellationToken),
                LoadRewardedAsync(cancellationToken)
            );
            _logger.Log("[ADX] 모든 광고 로드 완료");
        }


        // 초기화 됐는지 확인
        public bool IsInitialized =>_adxProvider.IsInitialized.CurrentValue;





        // IBackendAds 구현
        

        // BANNER
        public async Task LoadBannerAsync(BannerSize bannerSize, BannerPosition bannerPosition, CancellationToken cancellationToken = default)
        {
            await Banner.LoadBannerAsync(bannerSize, bannerPosition, cancellationToken);
        }

        public void HideBannerAd()
        {
            Banner.HideBannerAd();
        }



        //INTERSTITIAL
        public async Task LoadInterstitialAsync(CancellationToken cancellationToken = default)
        {
            await Interstitial.LoadInterstitialAsync(cancellationToken);
        }

        public void ShowInterstitialAd(Action onShown, Action onClose)
        {
            Interstitial.ShowInterstitialAd(onShown, onClose);
        }

        public bool IsInterstitialAdReady()
        {
            return Interstitial.IsInterstitialAdReady();
        }



        
        public async Task LoadRewardedAsync(CancellationToken cancellationToken = default)
        {
            await Rewarded.LoadRewardedAsync(cancellationToken);
        }

        public void ShowRewardedAd(Action<double> onRewarded)
        {
            Rewarded.ShowRewardedAd(onRewarded);
        }

        public bool IsRewardedAdReady()
        {
            return Rewarded.IsRewardedAdReady();
        }
    }
}
