using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using AdxUnityPlugin;
using R3;

namespace Ncroquis.Backend
{
    public class AdxBackendAds : IBackendAds, IDisposable
    {
        public ProviderKey providerKey => ProviderKey.ADX;

#if UNITY_ANDROID
        string adxBannerAdUnitId = "61ee2b7dcb8c67000100002a";
        string adxInterstitialAdUnitId = "61ee2e3fcb8c67000100002e";
        string adxRewardedAdUnitId = "61ee2e91cb8c67000100002f";
#elif UNITY_IPHONE
        string adxBannerAdUnitId = "6200fee42a918d0001000003";
        string adxInterstitialAdUnitId = "6200fef52a918d0001000007";
        string adxRewardedAdUnitId = "6200ff0c2a918d000100000d";
#endif

        private AdxBannerAd _bannerAd;
        private AdxInterstitialAd _interstitialAd;
        private AdxRewardedAd _rewardedAd;

        public event Action OnAdError;
        public event Action<string, double> OnAdRevenue;

        private readonly AdxBackendProvider _adxProvider;
        private readonly CompositeDisposable _disposables = new();
        private CancellationTokenSource _cts = new();

        [Inject]
        public AdxBackendAds(AdxBackendProvider provider)
        {
            _adxProvider = provider;

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

            _bannerAd?.Destroy();
            _bannerAd = null;

            _interstitialAd?.Destroy();
            _interstitialAd = null;

            _rewardedAd?.Destroy();
            _rewardedAd = null;

            Debug.Log("[ADX Ads] disposed and resources released.");
        }

        private async Task LoadAllAdsAsync(CancellationToken cancellationToken = default)
        {
            Debug.Log("[ADX Ads] ProviderAds is initialized. Loading all ads...");

            try
            {
                await Task.WhenAll(
                    LoadBannerAsync(cancellationToken),
                    LoadInterstitialAsync(cancellationToken),
                    LoadRewardedAsync(cancellationToken)
                );

                Debug.Log("[ADX Ads] All ads loaded successfully!");
            }
            catch (OperationCanceledException)
            {
                Debug.Log("[ADX Ads] Ad loading was cancelled");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ADX Ads] Failed to load ads: {ex.Message}");
            }
        }

#region BANNER

        public async Task LoadBannerAsync(CancellationToken cancellationToken = default)
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Ads] ADX SDK not initialized. Cannot load banner.");
                OnAdError?.Invoke();
                return;
            }

            _bannerAd?.Destroy();
            _bannerAd = new AdxBannerAd(adxBannerAdUnitId, AdxBannerAd.AD_SIZE_320x50, AdxBannerAd.POSITION_TOP);

            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded() => tcs.TrySetResult(true);
            void OnFailed(int error)
            {
                Debug.LogError($"[ADX Ads] Banner load failed: {error}");
                OnAdError?.Invoke();
                tcs.TrySetException(new Exception(error.ToString()));
            }

            _bannerAd.OnAdLoaded -= OnLoaded;
            _bannerAd.OnAdFailedToLoad -= OnFailed;

            _bannerAd.OnAdLoaded += OnLoaded;
            _bannerAd.OnAdFailedToLoad += OnFailed;

            // _bannerAd.OnAdClicked += () => Debug.Log("[ADX Ads] Banner clicked.");
            // _bannerAd.OnPaidEvent += (ecpm) =>
            // {
            //     double revenue = ecpm / 1000f;
            //     OnAdRevenue?.Invoke("ADX Banner Ad", revenue);
            // };

            _bannerAd.Load();
            Debug.Log("[ADX Ads] Banner load requested.");

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                await tcs.Task;
        }      

        public void HideBannerAd()
        {
            if (_bannerAd != null)
            {
                _bannerAd.Destroy();
                _bannerAd = null;
                Debug.Log("[ADX Ads] Banner ad hidden and destroyed.");
            }
        }

#endregion

#region INTERSTITIAL

        public async Task LoadInterstitialAsync(CancellationToken cancellationToken = default)
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Ads] ADX SDK not initialized. Cannot load interstitial.");
                OnAdError?.Invoke();
                return;
            }

            if (_interstitialAd == null)
            {
                _interstitialAd = new AdxInterstitialAd(adxInterstitialAdUnitId);
            }

            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded() => tcs.TrySetResult(true);
            void OnFailed(int error)
            {
                Debug.LogError($"[ADX Ads] Interstitial load failed: {error}");
                OnAdError?.Invoke();
                tcs.TrySetException(new Exception(error.ToString()));
            }

            _interstitialAd.OnAdLoaded -= OnLoaded;
            _interstitialAd.OnAdFailedToLoad -= OnFailed;

            _interstitialAd.OnAdLoaded += OnLoaded;
            _interstitialAd.OnAdFailedToLoad += OnFailed;

            // _interstitialAd.OnAdFailedToShow += () =>
            // {
            //     Debug.LogError("[ADX Ads] Interstitial failed to show.");
            //     OnAdError?.Invoke();
            // };
            // _interstitialAd.OnPaidEvent += (ecpm) =>
            // {
            //     double revenue = ecpm / 1000f;
            //     OnAdRevenue?.Invoke("ADX Interstitial Ad", revenue);
            // };

            _interstitialAd.Load();
            Debug.Log("[ADX Ads] Interstitial load requested.");

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                await tcs.Task;
        }
        private Action _pendingInterstitialCallback;

        public async void ShowInterstitialAd(Action onShown, Action onClose = default)
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Ads] ADX SDK not initialized. Cannot show interstitial.");
                OnAdError?.Invoke();
                return;
            }

            void HandleAdShown()
            {
                Debug.Log("[ADX Ads] Interstitial ad shown.");
                _interstitialAd.OnAdShown -= HandleAdShown;
                onShown?.Invoke();
            }

            void HandleAdClosed()
            {
                Debug.Log("[ADX Ads] Interstitial ad closed.");
                _interstitialAd.OnAdClosed -= HandleAdClosed;
                onClose?.Invoke();
            }

            if (_interstitialAd != null && _interstitialAd.IsLoaded())
            {
                _interstitialAd.OnAdShown -= HandleAdShown;
                _interstitialAd.OnAdShown += HandleAdShown;

                _interstitialAd.OnAdClosed -= HandleAdClosed;
                _interstitialAd.OnAdClosed += HandleAdClosed;

                _interstitialAd.Show();
                Debug.Log("[ADX Ads] Interstitial ad requested to show.");
            }
            else
            {
                Debug.Log("[ADX Ads] Interstitial not ready. Attempting to load...");
                _pendingInterstitialCallback = onShown;

                try
                {
                    await LoadInterstitialAsync();
                    if (_interstitialAd != null && _interstitialAd.IsLoaded())
                    {
                        _interstitialAd.OnAdShown -= HandleAdShown;
                        _interstitialAd.OnAdShown += HandleAdShown;

                        _interstitialAd.OnAdClosed -= HandleAdClosed;
                        _interstitialAd.OnAdClosed += HandleAdClosed;

                        _interstitialAd.Show();
                        Debug.Log("[ADX Ads] Interstitial ad shown after loading.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ADX Ads] Failed to load interstitial ad: {ex.Message}");
                    OnAdError?.Invoke();
                }
                finally
                {
                    _pendingInterstitialCallback = null;
                }
            }
        }

        public bool IsInterstitialAdReady()
        {
            return _interstitialAd != null && _interstitialAd.IsLoaded();
        }

#endregion

#region REWARDED

        public async Task LoadRewardedAsync(CancellationToken cancellationToken = default)
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Ads] ADX SDK not initialized. Cannot load rewarded ad.");
                OnAdError?.Invoke();
                return;
            }

            if (_rewardedAd == null)
            {
                _rewardedAd = new AdxRewardedAd(adxRewardedAdUnitId);
            }

            var tcs = new TaskCompletionSource<bool>();

            void OnLoaded() => tcs.TrySetResult(true);
            void OnFailed(int error)
            {
                Debug.LogError($"[ADX Ads] Rewarded load failed: {error}");
                OnAdError?.Invoke();
                tcs.TrySetException(new Exception(error.ToString()));
            }

            _rewardedAd.OnRewardedAdLoaded -= OnLoaded;
            _rewardedAd.OnRewardedAdFailedToLoad -= OnFailed;

            _rewardedAd.OnRewardedAdLoaded += OnLoaded;
            _rewardedAd.OnRewardedAdFailedToLoad += OnFailed;

            // _rewardedAd.OnRewardedAdFailedToShow += () =>
            // {
            //     Debug.LogError("[ADX Ads] Rewarded failed to show.");
            //     OnAdError?.Invoke();
            // };
            // _rewardedAd.OnPaidEvent += (ecpm) =>
            // {
            //     double revenue = ecpm / 1000f;
            //     OnAdRevenue?.Invoke("ADX Rewarded Ad", revenue);
            // };

            _rewardedAd.Load();
            Debug.Log("[ADX Ads] Rewarded load requested.");

            using (cancellationToken.Register(() => tcs.TrySetCanceled()))
                await tcs.Task;
        }

        private Action<double> _pendingRewardedCallback;

        public async void ShowRewardedAd(Action<double> onRewarded)
        {
            if (!_adxProvider.IsInitialized.CurrentValue)
            {
                Debug.LogError("[ADX Ads] ADX SDK not initialized. Cannot show rewarded ad.");
                OnAdError?.Invoke();
                return;
            }

            if (_rewardedAd != null && _rewardedAd.IsLoaded())
            {
                _rewardedAd.OnPaidEvent += onRewarded;
                _rewardedAd.Show();
                Debug.Log("[ADX Ads] Rewarded ad requested to show.");
            }
            else
            {
                Debug.Log("[ADX Ads] Rewarded ad not ready. Attempting to load...");
                _pendingRewardedCallback = onRewarded;

                try
                {
                    await LoadRewardedAsync();
                    if (_rewardedAd != null && _rewardedAd.IsLoaded() && _pendingRewardedCallback != null)
                    {
                        _rewardedAd.OnPaidEvent += _pendingRewardedCallback;
                        _rewardedAd.Show();
                        Debug.Log("[ADX Ads] Rewarded ad shown after loading.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ADX Ads] Failed to load rewarded ad: {ex.Message}");
                    OnAdError?.Invoke();
                }
                finally
                {
                    _pendingRewardedCallback = null;
                }
            }
        }

        public bool IsRewardedAdReady()
        {
            return _rewardedAd != null && _rewardedAd.IsLoaded();
        }

#endregion

    }
}
