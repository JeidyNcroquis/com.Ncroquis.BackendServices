using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Ncroquis.Backend;



public class BackendEntryPoint : IInitializable
{

    [Inject] protected readonly BackendService _backendService;


    public async void Initialize()
    {

        
        // 초기화 예시

        // () 는 기본 -> Firebase 설정
        await _backendService.Provider().InitializeAsync();


        // bool isSignedIn = await _backendService.Auth().SignInAnonymouslyAsync();
        // if (isSignedIn)
        // {

        //     // Analytics 이벤트 로깅 예시

        //     _backendService.Analytics().LogEvent("TEST_EVENT_WITH_PARAMS",
        //         new AnalyticsParameter("level_name", 1),
        //         new AnalyticsParameter("attempt_count", 2));



        //     // // 저장 예시
        //     // var profile = new PlayerProfile { Name = "UnityUser", Level = 1 };
        //     // bool success = await _backendService.Data().SetDocumentAsync("players", "user123", profile);
        //     // if (success)
        //     //     Debug.Log("Firestore에 플레이어 프로필 저장 성공!");
        //     // else
        //     //     Debug.LogError("Firestore에 플레이어 프로필 저장 실패");


        //     // // 불러오기 예시
        //     // var loadedProfile = await _backendService.Data().GetDocumentAsync<PlayerProfile>("players", "user123");
        //     // if (loadedProfile != null)
        //     // {
        //     //     Debug.Log($"불러온 프로필: 이름={loadedProfile.Name}, 레벨={loadedProfile.Level}");
        //     // }
        //     // else
        //     // {
        //     //     Debug.LogError("프로필 불러오기 실패");
        //     // }

        // }
    }
}


