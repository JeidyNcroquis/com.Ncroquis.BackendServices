
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Firebase.Analytics;


namespace Ncroquis.Backend
{

    /// <summary>
    /// Firebase Analytics를 사용하여 이벤트를 로깅하는 클래스입니다.
    /// </summary>
    public class FirebaseBackendAnalytics : IBackendAnalytics
    {
        /// <summary>
        /// 지정된 이벤트와 파라미터들을 Firebase Analytics에 로깅합니다.
        /// </summary>
        /// <param name="eventName">로깅할 이벤트의 이름입니다.</param>
        /// <param name="parameters">이벤트와 함께 전송할 파라미터 배열입니다.</param>
        public void LogEvent(string eventName, params AnalyticsParameter[] parameters)
        {
            // 파라미터가 없거나 null인 경우, 파라미터 없이 이벤트를 로깅합니다.
            if (parameters == null || parameters.Length == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                Debug.Log($"[Firebase Analytics] 이벤트 로깅: {eventName} (파라미터 없음)");
                return;
            }

            // 전달받은 AnalyticsParameter를 Firebase Analytics의 Parameter 객체로 변환합니다.
            var firebaseParams = new List<Parameter>();

            foreach (var param in parameters)
            {
                // Firebase Analytics는 string, long, double 타입의 파라미터만 지원합니다.
                // object 타입의 Value를 적절한 타입으로 변환합니다.
                switch (param.Value)
                {
                    case string stringValue:
                        firebaseParams.Add(new Parameter(param.Key, stringValue));
                        break;
                    case long longValue:
                        firebaseParams.Add(new Parameter(param.Key, longValue));
                        break;
                    case int intValue: // int는 long으로 변환
                        firebaseParams.Add(new Parameter(param.Key, intValue));
                        break;
                    case double doubleValue:
                        firebaseParams.Add(new Parameter(param.Key, doubleValue));
                        break;
                    case float floatValue: // float은 double로 변환
                        firebaseParams.Add(new Parameter(param.Key, floatValue));
                        break;
                    case bool boolValue: // bool은 long(1 또는 0)으로 변환
                        firebaseParams.Add(new Parameter(param.Key, boolValue ? 1L : 0L));
                        break;
                    default:
                        // 지원하지 않는 타입의 경우 경고를 출력하고 파라미터를 건너뜁니다.
                        // 또는 object.ToString()을 사용하여 문자열로 변환할 수도 있습니다.
                        Debug.LogWarning($"[Firebase Analytics] 지원하지 않는 파라미터 타입입니다. Key: '{param.Key}', Type: '{param.Value?.GetType().Name}'. 문자열로 변환하여 기록합니다.");
                        firebaseParams.Add(new Parameter(param.Key, param.Value?.ToString()));
                        break;
                }
            }

            // Firebase Analytics를 사용하여 이벤트 로깅
            FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());

            // 디버그 로그 출력 (원래 파라미터 값을 기준으로)
            string paramsString = string.Join(", ", parameters.Select(p => $"{p.Key}: {p.Value}"));
            Debug.Log($"[Firebase Analytics] 이벤트 로깅: {eventName}, 파라미터: [{paramsString}]");
        }
    }
}