

namespace Ncroquis.Backend
{

    // 백엔드 서비스 접근을 위한 클래스
    public class BackendService : ABackendService
    {

        // 키를 명시하지 않으면 기본값으로 첫 번째 등록된 백엔드 타입을 사용
        // 예) _backendService.Provider() 는 첫번째 등록한 백엔드로 사용

        public override IBackendProvider Provider(string key = null) => Get(_container.Providers, key, nameof(IBackendProvider));
        public override IBackendAuth Auth(string key = null) => Get(_container.Auths, key, nameof(IBackendAuth));
        public override IBackendAnalytics Analytics(string key = null) => Get(_container.Analytics, key, nameof(IBackendAnalytics));
        public override IBackendData Data(string key = null) => Get(_container.Datas, key, nameof(IBackendData));
        public override IBackendAds Ads(string key = null) => Get(_container.Ads, key, nameof(IBackendAds));
    }
}
