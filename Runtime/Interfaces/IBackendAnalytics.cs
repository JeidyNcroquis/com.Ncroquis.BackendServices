

namespace Ncroquis.Backend
{


    // 분석 이벤트 로깅

    public interface IBackendAnalytics : IBackendIdentifiable
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


}