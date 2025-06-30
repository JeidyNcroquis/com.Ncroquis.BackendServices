

namespace Ncroquis.Backend
{

    /// <summary>
    /// 사용법
    /// </summary>

    // 예1) - Collection 만 사용
    //backendservice.Analytics().LogEvent("EVENTLOG1");


    // 예2) Collection , DocumentID , Data 싱글
    // backendservice.Analytics().LogEvent("EVENTLOG2",
    //         new AnalyticsParameter("SELECT_CHARACTER", "WIZARD"));


    // 예3 - Collection , DocumentID , Data 배열
    // backendservice.Analytics().LogEvent("EVENTLOG3",
    //         new AnalyticsParameter("LEVEL", 5),
    //         new AnalyticsParameter("CLEAR_TIME", 42.7f),
    //         new AnalyticsParameter("IS_SUCCESS", true),
    //         new AnalyticsParameter("CURRENCY", "GOLD"));



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