using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class WebGLTestUI : MonoBehaviour

{
    [SerializeField] private TMP_Text logText;
    
    void Start()
    {
        UniWebViewBridge.OnRequestSuccess += OnRequestSuccess;
        UniWebViewBridge.OnRequestError += OnRequestError;
    }

    void OnDestroy()
    {
        UniWebViewBridge.OnRequestSuccess -= OnRequestSuccess;
        UniWebViewBridge.OnRequestError -= OnRequestError;
    }
    public void OnRequestSuccess(string json)
    {
        Log("← Request resolved: " + json);
    }

    public void OnRequestError(string error)
    {
        Log("← Request error: " + error);
    }

    private void Log(string msg)
    {
        Debug.Log(msg);
        if (logText != null)
            logText.text += "\n" + msg;
    }

    // private void OnTestSendString()
    // {
    //     UniWebViewBridge.Send("sendCurrencyReward", new SendCurrencyReward { currency = CurrencyType.Coin, amount = 1000 });
    //     Log("send → sendCurrencyReward: { currency: Coin, amount: 1000 }");
    // }

    // private void OnTestCall()
    // {
    //     var result = UniWebViewBridge.Call("getCurrencyTotal", new GetCurrencyTotal { currency = CurrencyType.Coin });
    //     Log($"call → getCurrencyTotal reply: {result}");
    // }

    // private void OnTestRequest()
    // {
    //     UniWebViewBridge.Request("loadStageLeaderboard", new GetEndlessStageKey { endlessStageKey = "season_week_2026_25" }, timeout: 60000);
    //     Log("request → loadStageLeaderboard pending...");
    // }


}