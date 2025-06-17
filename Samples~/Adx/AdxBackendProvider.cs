using System.Threading.Tasks;
using UnityEngine;
using AdxUnityPlugin; // ADX SDK 관련 클래스를 사용하기 위해 필요합니다.

namespace Ncroquis.Backend
{
    
    /// ADX 라이브러리의 백엔드 제공자(Provider) 구현체입니다.
    /// ADX SDK 초기화 및 전반적인 상태 관리를 담당합니다.
    /// "Initialize | ADX Library" 문서에 명시된 ADX SDK 초기화 로직을 따릅니다.
    public class AdxBackendProvider : IBackendProvider 
    {
        
        /// 이 제공자의 이름을 반환합니다. ADX 라이브러리의 이름에 기반합니다.        
        public string ProviderName => "ADX"; 

        
        /// SDK가 초기화되었는지 여부를 나타냅니다.        
        public bool IsInitialized { get; private set; } = false; 

        private readonly string _adxAppId;
        private readonly GdprType _gdprType;
        private TaskCompletionSource<bool> _initializeTcs;

        /// <summary>
        /// AdxBackendProvider의 새 인스턴스를 초기화합니다.
        /// ADX SDK 초기화에 필요한 애플리케이션 ID와 GDPR 타입을 전달받습니다.
        /// </summary>
        /// <param name="adxAppId">ADX 애플리케이션 ID ("<ADX_APP_ID>"에 해당)</param>
        /// <param name="gdprType">GDPR 동의 처리 방식 (POPUP_LOCATION, POPUP_DEBUG, DIRECT_NOT_REQUIRED, DIRECT_DENIED, DIRECT_CONFIRM 중 하나)</param>
        public AdxBackendProvider(string adxAppId, GdprType gdprType)
        {
            _adxAppId = adxAppId;
            _gdprType = gdprType;
        }

        /// <summary>
        /// ADX SDK를 비동기적으로 초기화합니다.
        /// SDK 초기화는 앱 실행 시 한 번만 호출되어야 하며, 광고 요청은 초기화 완료 후에 이루어져야 합니다.
        /// </summary>
        /// <returns>초기화 성공 여부를 나타내는 Task<bool></returns>
        public Task<bool> InitializeAsync() 
        {
            if (IsInitialized)
            {
                Debug.LogWarning("[ADX Backend Provider] ADX SDK는 이미 초기화되었습니다.");
                return Task.FromResult(true);
            }

            // 이미 초기화가 진행 중인 경우, 기존 Task를 반환합니다.
            if (_initializeTcs != null && !_initializeTcs.Task.IsCompleted)
            {
                Debug.LogWarning("[ADX Backend Provider] ADX SDK 초기화가 이미 진행 중입니다.");
                return _initializeTcs.Task;
            }

            _initializeTcs = new TaskCompletionSource<bool>();

            // QA 진행 시 연동 및 미디에이션 정상 동작 확인을 위해 초기화 함수 호출 전에 아래와 같이 추가되어야 합니다. 
            AdxSDK.SetLogEnable(true);

            // ADXConfiguration.Builder를 사용하여 구성 객체를 생성합니다. 
            ADXConfiguration adxConfiguration = new ADXConfiguration.Builder()
                .SetAppId(_adxAppId) // ADX에서 발급받은 App ID 사용 
                .SetGdprType(_gdprType) // GDPR 타입 설정 
                .Build();

            // AdxSDK.Initialize를 호출하고 콜백 메서드인 OnADXConsentCompleted를 전달합니다.
            // OnADXConsentCompleted가 호출된 후, 광고를 요청해야 합니다. 
            AdxSDK.Initialize(adxConfiguration, OnADXConsentCompleted); 


            return _initializeTcs.Task;
        }

        /// <summary>
        /// AdxSDK.Initialize 호출 후 ADX 동의 절차가 완료될 때 호출되는 콜백 메서드입니다. 
        /// 이 콜백이 호출된 후, 광고 관련 로직을 진행해야 합니다.
        /// </summary>
        /// <param name="s">동의 완료 상태를 나타내는 문자열 (예: "editor (simulated)" 등)</param>
        private void OnADXConsentCompleted(string s)
        {
            Debug.LogFormat("[ADX Backend Provider] ADX 동의 완료: {0}", s);

            // "광고 요청은 초기화가 완료된 후" 이루어져야 하므로, 이 시점에서 초기화 완료 상태를 설정합니다.
            IsInitialized = true;
            _initializeTcs?.TrySetResult(true); // TaskCompletionSource를 통해 Task를 성공으로 완료합니다.
        }
    }
}