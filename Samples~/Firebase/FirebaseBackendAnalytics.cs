using System.Collections.Generic;
using Firebase.Analytics;

namespace Ncroquis.Backend
{
    public class FirebaseBackendAnalytics : IBackendAnalytics
    {
        public ProviderKey providerKey => ProviderKey.FIREBASE;


        public void LogEvent(string eventName, params AnalyticsParameter[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
            {
                FirebaseAnalytics.LogEvent(eventName);
                return;
            }

            var firebaseParams = new List<Parameter>(parameters.Length);

            foreach (var param in parameters)
            {
                var value = param.Value;

                firebaseParams.Add(value switch
                {
                    string s => new Parameter(param.Key, s),
                    long l => new Parameter(param.Key, l),
                    int i => new Parameter(param.Key, i),
                    double d => new Parameter(param.Key, d),
                    float f => new Parameter(param.Key, f),
                    bool b => new Parameter(param.Key, b ? 1L : 0L),
                    _ => new Parameter(param.Key, value?.ToString() ?? string.Empty)
                });
            }

            FirebaseAnalytics.LogEvent(eventName, firebaseParams.ToArray());
        }
    }
}
