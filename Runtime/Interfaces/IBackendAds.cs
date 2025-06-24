using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ncroquis.Backend
{
    public enum BannerSize
    {
        Size_320x50,
        Size_728x90,
        Size_320x100,
        Size_300x250
    }

    public enum BannerPosition
    {
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }

    /// <summary>
    /// ADX 라이브러리가 지원하는 광고 형식에 기반한 광고 기능 인터페이스입니다.
    /// </summary>
    public interface IBackendAds : IBackendIdentifiable
    {
        /// <summary>
        /// 배너 광고를 지정된 크기와 위치로 로드합니다. 로드 후 자동 노출
        /// </summary>
        Task LoadBannerAsync(BannerSize bannerSize, BannerPosition bannerPosition, CancellationToken cancellationToken = default);

        /// <summary>
        /// 표시된 배너 광고를 숨깁니다.
        /// </summary>
        void HideBannerAd();

        /// <summary>
        /// 전면 광고를 로드합니다.
        /// </summary>
        Task LoadInterstitialAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 로드된 전면 광고를 표시합니다.
        /// </summary>
        void ShowInterstitialAd(Action onShown, Action onClose);

        /// <summary>
        /// 전면 광고가 로드되어 표시 준비가 되었는지 확인합니다.
        /// </summary>
        bool IsInterstitialAdReady();

        /// <summary>
        /// 보상형 광고를 로드합니다.
        /// </summary>
        Task LoadRewardedAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// 로드된 보상형 광고를 표시합니다.
        /// </summary>
        void ShowRewardedAd(Action onRewarded);

        /// <summary>
        /// 보상형 광고가 로드되어 표시 준비가 되었는지 확인합니다.
        /// </summary>
        bool IsRewardedAdReady();

        /// <summary>
        /// 광고 로드 또는 표시 중 오류가 발생할 때 발생하는 이벤트입니다.
        /// </summary>
        event Action OnAdError;

        /// <summary>
        /// 광고로부터 수익이 발생했을 때 발생하는 이벤트입니다.
        /// </summary>
        event Action<string, double> OnAdRevenue;
    }
}
