using System.Threading.Tasks;
using UnityEngine;



namespace Ncroquis.Backend
{


    //인증 관련 기능 인터페이스

    public interface IBackendAuth : IBackendIdentifiable
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



}