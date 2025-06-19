
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using AdxUnityPlugin;
using R3;



namespace Ncroquis.Backend
{
    
    
    /// ADX 라이브러리의 광고 기능 인터페이스(IBackendAds) 구현체    
    public class AdxBackendAds : IBackendAds, IDisposable
    {        
        public ProviderKey providerKey => ProviderKey.ADX;

#if UNITY_ANDROID
        string adxBannerAdUnitId = "61ee2b7dcb8c67000100002a"; //테스트용
        string adxInterstitialAdUnitId = "61ee2e3fcb8c67000100002e"; //테스트용
        string adxRewardedAdUnitId = "61ee2e91cb8c67000100002f"; //테스트용
#elif UNITY_IPHONE
        string adxBannerAdUnitId = "6200fee42a918d0001000003"; //테스트용
        string adxInterstitialAdUnitId = "6200fef52a918d0001000007"; //테스트용
        string adxRewardedAdUnitId = "6200ff0c2a918d000100000d"; //테스트용    
#endif


        // ADX 광고 인스턴스
        private AdxBannerAd _bannerAd;
        private AdxInterstitialAd _interstitialAd;
        private AdxRewardedAd _rewardedAd;        


        // IBackendAds 인터페이스 이벤트 [17, 18]
        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue; // 소스 설명을 반영하여 Action<string, double>으로 변경 [18]




        private readonly AdxBackendProvider _adxProvider;
        private readonly CompositeDisposable _disposables = new();
        private CancellationTokenSource _cts = new();

        
        [Inject]
        public AdxBackendAds(AdxBackendProvider provider)
        {
            _adxProvider = provider;

            // ProviderAds만 초기화되면 광고 로드
            _adxProvider.IsInitialized
                .Where(isInitialized => isInitialized)
                .Subscribe(async _ => await LoadAllAdsAsync(_cts.Token))
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _disposables?.Dispose();

            Debug.Log("Ads disposed and resources released.");
        }




        private async Task LoadAllAdsAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("ProviderAds is initialized. Loading all ads...");

            try
            {
                await Task.WhenAll(
                    LoadBannerAsync(cancellationToken),
                    LoadInterstitialAsync(cancellationToken),
                    LoadRewardedAsync(cancellationToken)
                );

                Debug.Log("All ads loaded successfully!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Ad loading was cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load ads: {ex.Message}");
            }
        }






        ///
        /// 배너 광고를 로드합니다.
        /// ADX 배너 광고는 크기와 위치를 생성자에 명시해야 합니다.
        /// IBackendAds 인터페이스는 이러한 파라미터를 제공하지 않으므로, 기본값(320x50, 상단)을 사용합니다.
        ///
        /// 광고 단위 ID
        public async Task LoadBannerAsync(CancellationToken cancellationToken = default)
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 배너 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            _bannerAd?.Destroy();
            _bannerAd = null;
            Debug.Log("[ADX Backend Ads] 기존 배너 광고를 파괴하고 다시 로드합니다.");

            var tcs = new TaskCompletionSource<bool>();

            _bannerAd = new AdxBannerAd(adxBannerAdUnitId, AdxBannerAd.AD_SIZE_320x50, AdxBannerAd.POSITION_TOP);

            _bannerAd.OnAdLoaded += () =>
            {
                Debug.Log("[ADX Backend Ads] 배너 광고 로드 성공.");
                tcs.TrySetResult(true);
            };

            _bannerAd.OnAdFailedToLoad += (error) =>
            {
                Debug.LogError($"[ADX Backend Ads] 배너 광고 로드 실패: {error}");
                OnAdError?.Invoke();
                tcs.TrySetException(new Exception(error.ToString()));
            };

            _bannerAd.OnAdClicked += () =>
            {
                Debug.Log("[ADX Backend Ads] 배너 광고 클릭됨.");
            };

            _bannerAd.OnPaidEvent += (ecpm) =>
            {
                double revenue = ecpm / 1000f;
                Debug.Log($"[ADX Backend Ads] 배너 광고 수익 발생: {revenue} USD (eCPM: {ecpm})");
                OnAdRevenue?.Invoke("ADX Banner Ad", revenue);
            };

            _bannerAd.Load();
            Debug.Log($"[ADX Backend Ads] 배너 광고 로드 요청: {adxBannerAdUnitId}");

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
            {
                await tcs.Task;
            }
        }


        ///
        /// 로드된 배너 광고를 표시합니다.
        /// ADX 배너 광고는 Load() 시 자동으로 표시되므로, 이 메서드는 단순히 광고가 로드되었는지 확인하는 역할을 합니다.
        ///
        public void ShowBannerAd()
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 배너 광고를 표시할 수 없습니다."); // [10]
                OnAdError?.Invoke(); // [21]
                return;
            }

            if (_bannerAd != null)
            {
                Debug.Log("[ADX Backend Ads] 배너 광고가 이미 로드되어 있거나 로드 요청되었습니다. (ADX 배너 광고는 Load() 시 표시됩니다.)"); // [21]
            }
            else
            {
                Debug.LogWarning("[ADX Backend Ads] 표시할 배너 광고 인스턴스가 없습니다. LoadBannerAd를 먼저 호출해야 합니다."); // [21]
                OnAdError?.Invoke();
            }
        }

        ///
        /// 표시된 배너 광고를 숨깁니다.
        /// ADX SDK에서는 배너 광고 인스턴스를 파괴함으로써 숨기는 효과를 냅니다.
        ///
        public void HideBannerAd()
        {
            if (_bannerAd != null) // [11]
            {
                _bannerAd.Destroy(); // 광고 인스턴스 파괴
                _bannerAd = null;
                Debug.Log("[ADX Backend Ads] 배너 광고를 숨기고 파괴했습니다."); // [11]
            }
            else
            {
                Debug.LogWarning("[ADX Backend Ads] 파괴할 배너 광고 인스턴스가 없습니다."); // [11]
            }
        }


        ///
        /// 전면 광고를 로드합니다.
        /// 
        public async Task LoadInterstitialAsync(CancellationToken cancellationToken = default)
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 전면 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            if (_interstitialAd == null)
            {
                _interstitialAd = new AdxInterstitialAd(adxInterstitialAdUnitId);

                var tcs = new TaskCompletionSource<bool>();

                _interstitialAd.OnAdLoaded += () =>
                {
                    Debug.Log("[ADX Backend Ads] 전면 광고 로드 성공.");
                    tcs.TrySetResult(true);
                };

                _interstitialAd.OnAdFailedToLoad += (error) =>
                {
                    Debug.LogError($"[ADX Backend Ads] 전면 광고 로드 실패: {error}");
                    OnAdError?.Invoke();
                    tcs.TrySetException(new Exception(error.ToString()));
                };

                _interstitialAd.OnAdClicked += () => Debug.Log("[ADX Backend Ads] 전면 광고 클릭됨.");
                _interstitialAd.OnAdShown += () => Debug.Log("[ADX Backend Ads] 전면 광고 표시됨.");
                _interstitialAd.OnAdClosed += () => Debug.Log("[ADX Backend Ads] 전면 광고 닫힘.");
                _interstitialAd.OnAdFailedToShow += () =>
                {
                    Debug.LogError($"[ADX Backend Ads] 전면 광고 표시 실패");
                    OnAdError?.Invoke();
                };

                _interstitialAd.OnPaidEvent += (ecpm) =>
                {
                    double revenue = ecpm / 1000f;
                    Debug.Log($"[ADX Backend Ads] 전면 광고 수익 발생: {revenue} USD (eCPM: {ecpm})");
                    OnAdRevenue?.Invoke("ADX Interstitial Ad", revenue);
                };

                _interstitialAd.Load();
                Debug.Log($"[ADX Backend Ads] 전면 광고 로드 요청: {adxInterstitialAdUnitId}");

                using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                {
                    await tcs.Task;
                }
            }
        }


        ///
        /// 로드된 전면 광고를 표시합니다.
        ///
        public void ShowInterstitialAd()
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 전면 광고를 표시할 수 없습니다."); // [23]
                OnAdError?.Invoke();
                return;
            }

            if (_interstitialAd != null && _interstitialAd.IsLoaded()) // 광고가 로드되어 준비되었는지 확인 [24]
            {
                _interstitialAd.Show(); // 광고 표시 [23]
                Debug.Log("[ADX Backend Ads] 전면 광고 표시 요청.");
            }
            else
            {
                Debug.LogWarning("[ADX Backend Ads] 전면 광고가 로드되지 않았거나 아직 준비되지 않았습니다."); // [23]
                OnAdError?.Invoke(); // 오류 이벤트 트리거
            }
        }

        ///
        /// 전면 광고가 로드되어 표시 준비가 되었는지 확인합니다.
        ///
        /// 광고 준비 여부
        public bool IsInterstitialAdReady()
        {
            return _interstitialAd != null && _interstitialAd.IsLoaded(); // [13]
        }



        ///
        /// 보상형 광고를 로드합니다.                
        /// 
        public async Task LoadRewardedAsync(CancellationToken cancellationToken = default)
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 보상형 광고를 로드할 수 없습니다.");
                OnAdError?.Invoke();
                return;
            }

            if (_rewardedAd == null)
            {
                _rewardedAd = new AdxRewardedAd(adxRewardedAdUnitId);
                var tcs = new TaskCompletionSource<bool>();

                _rewardedAd.OnRewardedAdLoaded += () =>
                {
                    Debug.Log("[ADX Backend Ads] 보상형 광고 로드 성공.");
                    tcs.TrySetResult(true);
                };

                _rewardedAd.OnRewardedAdFailedToLoad += (error) =>
                {
                    Debug.LogError($"[ADX Backend Ads] 보상형 광고 로드 실패: {error}");
                    OnAdError?.Invoke();
                    tcs.TrySetException(new Exception(error.ToString()));
                };

                _rewardedAd.OnRewardedAdShown += () => Debug.Log("[ADX Backend Ads] 보상형 광고 표시됨.");
                _rewardedAd.OnRewardedAdClicked += () => Debug.Log("[ADX Backend Ads] 보상형 광고 클릭됨.");
                _rewardedAd.OnRewardedAdClosed += () => Debug.Log("[ADX Backend Ads] 보상형 광고 닫힘.");
                _rewardedAd.OnRewardedAdFailedToShow += () =>
                {
                    Debug.LogError($"[ADX Backend Ads] 보상형 광고 표시 실패");
                    OnAdError?.Invoke();
                };
                _rewardedAd.OnRewardedAdEarnedReward += () =>
                {
                    Debug.Log("[ADX Backend Ads] 보상형 광고 보상 획득 이벤트 발생.");
                };

                _rewardedAd.OnPaidEvent += (ecpm) =>
                {
                    double revenue = ecpm / 1000f;
                    Debug.Log($"[ADX Backend Ads] 보상형 광고 수익 발생: {revenue} USD (eCPM: {ecpm})");
                    OnAdRevenue?.Invoke("ADX Rewarded Ad", revenue);
                };

                _rewardedAd.Load();
                Debug.Log($"[ADX Backend Ads] 보상형 광고 로드 요청: {adxRewardedAdUnitId}");

                using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                {
                    await tcs.Task;
                }
            }
        }


        ///
        /// 로드된 보상형 광고를 표시합니다.
        ///
        public void ShowRewardedAd()
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Backend Ads] ADX SDK가 초기화되지 않았습니다. 보상형 광고를 표시할 수 없습니다."); // [14]
                OnAdError?.Invoke();
                return;
            }

            if (_rewardedAd != null && _rewardedAd.IsLoaded()) // 광고가 로드되어 준비되었는지 확인 [26]
            {
                _rewardedAd.Show(); // 광고 표시 [26]
                Debug.Log("[ADX Backend Ads] 보상형 광고 표시 요청.");
            }
            else
            {
                Debug.LogWarning("[ADX Backend Ads] 보상형 광고가 로드되지 않았거나 아직 준비되지 않았습니다."); // [26]
                OnAdError?.Invoke(); // 오류 이벤트 트리거
            }
        }

        ///
        /// 보상형 광고가 로드되어 표시 준비가 되었는지 확인합니다.
        ///
        /// 광고 준비 여부
        public bool IsRewardedAdReady()
        {
            return _rewardedAd != null && _rewardedAd.IsLoaded(); // [26]
        }


    }
}