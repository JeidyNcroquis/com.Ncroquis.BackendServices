using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using R3;


namespace Ncroquis.Backend
{
    /// <summary>
    /// 공통 백엔드 Provider 초기화 로직을 관리하는 추상 베이스 클래스.
    /// 자식 클래스는 후처리(OnAfterAllInitialized)를 통해 커스텀 확장 가능.
    /// </summary>
    public abstract class AProvidersInitializerBase : IAsyncStartable
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<IBackendProvider> _providers;
        private readonly ReactiveProperty<bool> _allInitialized = new(false);

        /// <summary>
        /// 모든 Provider가 초기화 완료됐는지 나타내는 상태 값.
        /// </summary>
        public ReadOnlyReactiveProperty<bool> AllInitialized => _allInitialized;

        /// <summary>
        /// 생성자: DI로 주입받은 IBackendProvider들을 받아 상태 관찰 설정.
        /// </summary>
        [Inject]
        protected AProvidersInitializerBase(IEnumerable<IBackendProvider> providers, ILogger logger)
        {
            _logger = logger;
            _providers = providers;
            ObserveAllProvidersInitialized();
        }

        /// <summary>
        /// 모든 Provider의 IsInitialized 상태를 관찰하고
        /// 전부 true가 되면 AllInitialized 값을 true로 설정.
        /// </summary>
        private void ObserveAllProvidersInitialized()
        {
            if (!_providers.Any())
            {
                _allInitialized.Value = true;
                return;
            }

            var observables = _providers
                .Select(p => p.IsInitialized.AsObservable())
                .ToArray();

            Observable.CombineLatest<bool>(observables)
                .Select(states => states.All(b => b))
                .Subscribe(isAllInitialized =>
                {
                    _allInitialized.Value = isAllInitialized;

                    if (isAllInitialized)
                        _logger.Log("[ProvidersInitializer] 모든 Provider가 성공적으로 초기화되었습니다!");
                });
        }

        /// <summary>
        /// VContainer의 EntryPoint로 자동 실행되는 초기화 메서드.
        /// 모든 Provider를 병렬로 초기화한 후 후처리를 호출.
        /// </summary>
        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {
            _logger.Log("[ProvidersInitializer] Provider 초기화를 시작합니다...");

            var tasks = _providers.Select(p => p.InitializeAsync()).ToArray();

            try
            {
                await Task.WhenAll(tasks); // 모든 초기화를 병렬 실행
            }
            catch (Exception ex)
            {
                _logger.LogError($"[ProvidersInitializer] Provider 초기화 중 오류 발생: {ex.Message}");
                throw;
            }

            // 모든 Provider의 IsInitialized 상태가 true가 될 때까지 대기
            await AllInitialized
                .Where(initialized => initialized)
                .FirstAsync(cancellation);

            _logger.Log("[ProvidersInitializer] 모든 Provider 초기화가 완료되었습니다.");

            // 자식 클래스에서 정의한 후처리 호출
            await OnAfterAllInitialized(cancellation);
        }

        /// <summary>
        /// 모든 Provider 초기화 이후 실행될 커스텀 후처리 로직.
        /// 반드시 자식 클래스에서 구현 필요.
        /// </summary>
        protected abstract Task OnAfterAllInitialized(CancellationToken cancellation);
    }
}
