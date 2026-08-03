using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;
using Extras;
using I2.Loc;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Forgehub.SpookyBubbles;

public class FlagGetter : MonoBehaviour
{

    [SerializeField] public TMP_Text FlagAmount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void GetFlagTicket()
    {
        var raw = UniWebViewBridge.Call("getFlagTicketBalance", null);
        if (string.IsNullOrEmpty(raw))
        {
            OnFlagTicketChange(0);
            return;
        }

        var data = JsonConvert.DeserializeObject<FlagTicketBalanceResponse>(raw);
        if (data == null)
        {
            OnFlagTicketChange(0);
            return;
        }

        OnFlagTicketChange(data.balance);
    }

    public void OnFlagTicketChange(int value)
    {
        flagAmount = value;
        FlagAmount.text = "<sprite name=flag>" + flagAmount.ToString();
    }


    public void IncreaseFlagTicket()
    {
        UniWebViewBridge.Request("increaseFlagTicket", new IncreaseFlagTicketRequest { amount = 10 },
        onSuccess: json =>
        {
            var data = JsonConvert.DeserializeObject<FlagTicketBalanceResponse>(json);
            if (data != null)
                OnFlagTicketChange(data.balance);
        },
        onError: err => Debug.Log("increase error: " + err),
        timeout: 10000);
    }
}
