using UnityEngine;
using MessagePipe;
using VContainer;

namespace Ncroquis.Backend
{
    public readonly struct FocusChangedMessage
    {
        public readonly bool HasFocus;

        public FocusChangedMessage(bool hasFocus) => HasFocus = hasFocus;
    }

    public readonly struct PauseChangedMessage
    {
        public readonly bool IsPaused;

        public PauseChangedMessage(bool isPaused) => IsPaused = isPaused;
    }


    public class BackendServiceAppWatcher : MonoBehaviour
    {
        [Inject] private IPublisher<FocusChangedMessage> _focusPublisher;
        [Inject] private IPublisher<PauseChangedMessage> _pausePublisher;

        private void OnApplicationFocus(bool hasFocus)
        {
            _focusPublisher.Publish(new FocusChangedMessage(hasFocus));
        }

        private void OnApplicationPause(bool isPaused)
        {
            _pausePublisher.Publish(new PauseChangedMessage(isPaused));
        }
    }
}
