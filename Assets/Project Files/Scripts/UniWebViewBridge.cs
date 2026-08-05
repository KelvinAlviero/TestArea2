using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using Newtonsoft.Json;
using UnityEngine;

public class UniWebViewBridge : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RegisterRequestCallbacks(Action<string> onSuccess, Action<string> onError);

    [DllImport("__Internal")]
    private static extern void SendChannelMessage(string action, string payload);

    [DllImport("__Internal")]
    private static extern string CallChannelMessage(string action, string payload);

    [DllImport("__Internal")]
    private static extern void RequestChannelMessage(string action, string payload, int timeout);
#endif

    private class PendingRequest
    {
        public Action<string> OnSuccess;
        public Action<string> OnError;
    }

    private static readonly Queue<PendingRequest> pendingRequests = new();
    private static readonly object pendingLock = new();

    public static event Action OnStartLoading;
    public static event Action OnDoneLoading;

    private void Awake()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        RegisterRequestCallbacks(HandleSuccess, HandleError);
#endif
    }

    [MonoPInvokeCallback(typeof(Action<string>))]
    private static void HandleSuccess(string json)
    {
        var pending = DequeuePending();
        pending?.OnSuccess?.Invoke(json);
    }

    [MonoPInvokeCallback(typeof(Action<string>))]
    private static void HandleError(string error)
    {
        var pending = DequeuePending();
        pending?.OnError?.Invoke(error);
    }

    private static PendingRequest DequeuePending()
    {
        lock (pendingLock)
        {
            return pendingRequests.Count > 0 ? pendingRequests.Dequeue() : null;
        }
    }

    private static void EnqueuePending(Action<string> onSuccess, Action<string> onError)
    {
        lock (pendingLock)
        {
            pendingRequests.Enqueue(new PendingRequest
            {
                OnSuccess = onSuccess,
                OnError = onError
            });
        }
    }

    public static void Send(string action, object payload = null)
    {
        var json = payload == null ? "null" : JsonConvert.SerializeObject(payload);
#if UNITY_WEBGL && !UNITY_EDITOR
        SendChannelMessage(action, json);
#else
        Debug.Log($"[SIMULATED] send({action}, {json})");
#endif
    }

    public static string Call(string action, object payload = null)
    {
        var json = payload == null ? "null" : JsonConvert.SerializeObject(payload);
#if UNITY_WEBGL && !UNITY_EDITOR
        return CallChannelMessage(action, json);
#else
        Debug.Log($"[SIMULATED] call({action}, {json})");
        return null;
#endif
    }

    public static void Request(
        string action,
        object payload = null,
        Action<string> onSuccess = null,
        Action<string> onError = null,
        int timeout = 5000)
    {
        var json = payload == null ? "null" : JsonConvert.SerializeObject(payload);
        OnStartLoading?.Invoke();

        EnqueuePending(
            result =>
            {
                OnDoneLoading?.Invoke();
                onSuccess?.Invoke(result);
            },
            error =>
            {
                OnDoneLoading?.Invoke();
                onError?.Invoke(error);
            });

#if UNITY_WEBGL && !UNITY_EDITOR
        RequestChannelMessage(action, json, timeout);
#else
        Debug.Log($"[SIMULATED] request({action}, {json}, {timeout}ms)");
        OnDoneLoading?.Invoke();
        onSuccess?.Invoke("{\"simulated\":true}");
#endif
    }
}