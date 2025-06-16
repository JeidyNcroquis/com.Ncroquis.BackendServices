using UnityEngine;



namespace Ncroquis.Backend
{


    // 분석 이벤트 로깅

    public interface IBackendAnalytics
    {
        void LogEvent(string eventName, params AnalyticsParameter[] parameters);
    }

    public readonly struct AnalyticsParameter
    {
        public string Key { get; }
        public object Value { get; }

        public AnalyticsParameter(string key, object value)
        {
            Key = key;
            Value = value;
        }
    }


    // NULL 용 - 스텁 클래스 
    public class NullBackendAnalytics : IBackendAnalytics
    {
        public void LogEvent(string eventName, params AnalyticsParameter[] parameters)
        {
            Debug.LogWarning($"[Analytics] 이벤트 로깅 구현체가 없습니다: {eventName}");
        }
    }
}