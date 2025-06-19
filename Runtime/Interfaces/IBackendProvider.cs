using System.Threading;
using System.Threading.Tasks;
using UnityEngine; // UnityEngine 네임스페이스는 현재 이 클래스에서 직접 사용되지는 않지만, 프로젝트 구조상 포함되어 있는 것으로 보입니다.
using R3;


namespace Ncroquis.Backend
{

    //백엔드 초기화

    public interface IBackendProvider : IBackendIdentifiable
    {
        ReadOnlyReactiveProperty<bool> IsInitialized { get; }

        Task InitializeAsync(CancellationToken cancellation = default);
    }


    // NULL - 스텁 클래스
    public class NullBackendProvider : IBackendProvider
    {
        // NullProvider의 초기화 상태를 나타내는 ReactiveProperty
        // NullProvider는 실제 작업이 없으므로 항상 초기화된 것으로 간주합니다.
        private readonly ReactiveProperty<bool> _isInitialized;

        public NullBackendProvider()
        {
            // NullProvider는 초기화할 것이 없으므로 항상 true로 설정합니다.
            _isInitialized = new ReactiveProperty<bool>(true);
        }


        /// 제공자의 이름을 반환합니다. NullProvider임을 나타냅니다.        
        public ProviderKey providerKey => ProviderKey.NONE;

        /// 제공자가 초기화되었는지 여부를 나타내는 읽기 전용 반응형 속성입니다.
        /// NullProvider는 항상 초기화된 상태로 간주됩니다.        
        public ReadOnlyReactiveProperty<bool> IsInitialized => _isInitialized.ToReadOnlyReactiveProperty();

        
        /// 비동기 초기화 작업을 수행합니다. NullProvider는 아무런 초기화 작업도 수행하지 않고 즉시 완료됩니다.        
        /// <param name="cancellation">취소 토큰 (NullProvider에서는 사용되지 않음).</param>        
        public Task InitializeAsync(CancellationToken cancellation = default)
        {
            Debug.Log($"[{providerKey}] 초기화 성공");

            // NullProvider는 초기화할 것이 없으므로 즉시 완료된 Task를 반환합니다.
            return Task.CompletedTask;
        }
    }


}
