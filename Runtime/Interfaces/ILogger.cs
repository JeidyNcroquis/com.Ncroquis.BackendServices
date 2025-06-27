using UnityEngine;

namespace Ncroquis.Backend
{
    
    // 로그 레벨 정의
    // - Info   : 모든 로그(Info, Warning, Error)가 출력됨
    // - Warning: Warning과 Error 로그만 출력(Info는 출력되지 않음)
    // - Error  : Error 로그만 출력(Info, Warning은 출력되지 않음)
    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    // 로거 인터페이스
    public interface ILogger
    {
        LogLevel Level { get; set; }
        void Log(string message);
        void Warning(string message);
        void Error(string message);
    }

    // 유니티 기본 로거 구현
    public class UnityLogger : ILogger
    {
        public LogLevel Level { get; set; } = LogLevel.Info;

        private string COLOR = "white";

        public UnityLogger(LogLevel level = LogLevel.Info)
        {
            Level = level;
        }

        public void Log(string message)
        {
            if (Level > LogLevel.Info) return;
            Debug.Log($"<color={COLOR}>{message}</color>");
        }

        public void Warning(string message)
        {
            if (Level > LogLevel.Warning) return;
            Debug.LogWarning(message);
        }

        public void Error(string message)
        {
            if (Level > LogLevel.Error) return;
            Debug.LogError(message);
        }
    }
}
