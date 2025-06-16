using System.Threading.Tasks;
using UnityEngine;


namespace Ncroquis.Backend
{

    //백엔드 전체 초기화 및 상태 관리

    public interface IBackendProvider
    {
        string ProviderName { get; }
        bool IsInitialized { get; }
        Task<bool> InitializeAsync();
    }



    // NULL - 스텁 클래스
    public class NullBackendProvider : IBackendProvider
    {
        public string ProviderName => "NullProvider";
        public bool IsInitialized => false;

        public Task<bool> InitializeAsync()
        {
            Debug.LogWarning("IBackendProvider] 구현체가 없습니다");
            return Task.FromResult(false);
        }
    }


}