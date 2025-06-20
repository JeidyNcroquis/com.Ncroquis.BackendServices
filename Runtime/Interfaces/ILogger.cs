using UnityEngine;

namespace Ncroquis.Backend
{
    // 로거 인터페이스
    public interface ILogger
    {
        bool IsEnabled { get; set; }    // 토글 기능
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }

    // 유니티 기본 로거 구현
    public class UnityLogger : ILogger
    {
        public bool IsEnabled { get; set; } = true;

        public UnityLogger(bool isEnabled = true)
        {
            IsEnabled = isEnabled;
        }

        public void Log(string message)
        {
            if (IsEnabled)
                Debug.Log(message);
        }

        public void LogWarning(string message)
        {
            if (IsEnabled)
                Debug.LogWarning(message);
        }

        public void LogError(string message)
        {
            if (IsEnabled)
                Debug.LogError(message);
        }
    }
}
