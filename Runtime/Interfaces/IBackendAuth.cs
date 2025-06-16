using System.Threading.Tasks;
using UnityEngine;



namespace Ncroquis.Backend
{


    //인증 관련 기능 인터페이스

    public interface IBackendAuth
    {
        bool IsSignedIn { get; }
        string UserId { get; }

        // 익명 로그인
        Task<bool> SignInAnonymouslyAsync();

        // EMail 로그인 및 회원가입
        Task<bool> SignInWithEmailAsync(string email, string password);
        Task<bool> SignUpWithEmailAsync(string email, string password);
        Task SignOutAsync();


        void AddAuthStateChangedListener(System.Action<AuthStateEventArgs> listener);
        void RemoveAuthStateChangedListener(System.Action<AuthStateEventArgs> listener);
    }




    public class AuthStateEventArgs
    {
        public string UserId { get; set; }
        public bool IsSignedIn { get; set; }
    }


    // NULL - 스텁 클래스

    public class NullBackendAuth : IBackendAuth
    {
        public bool IsSignedIn => false;
        public string UserId => null;


        public Task<bool> SignInAnonymouslyAsync()
        {
            Debug.LogWarning("IBackendAuth] 구현체가 없습니다");
            return Task.FromResult(false);
        }

        public Task<bool> SignInWithEmailAsync(string email, string password)
        {
            Debug.LogWarning("IBackendAuth] 구현체가 없습니다");
            return Task.FromResult(false);
        }

        public Task<bool> SignUpWithEmailAsync(string email, string password)
        {
            Debug.LogWarning("IBackendAuth] 구현체가 없습니다");
            return Task.FromResult(false);
        }

        public Task SignOutAsync()
        {
            Debug.LogWarning("IBackendAuth] 구현체가 없습니다");
            return Task.CompletedTask;
        }

        public void AddAuthStateChangedListener(System.Action<AuthStateEventArgs> listener)
        {
            Debug.LogWarning("IBackendAuth] 구현체가 없습니다");
        }

        public void RemoveAuthStateChangedListener(System.Action<AuthStateEventArgs> listener)
        {
            Debug.LogWarning("IBackendAuth] 구현체가 없습니다");
        }
    }


}