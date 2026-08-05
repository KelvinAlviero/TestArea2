using System;


    public static class LoadingEvent
    {
        public static event Action OnStartLoadingEvent;

        public static event Action OnDoneLoadingEvent;

        public static void BroadcastStartLoading()
        {
            OnStartLoadingEvent?.Invoke();
        }

        public static void BroadcastDoneLoading()
        {
            OnDoneLoadingEvent?.Invoke();
        }
    }
