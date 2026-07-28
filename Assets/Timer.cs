using System;
using System.Text;
using Extras;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public UIWheelSpin uIWheelSpin;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private DateTime timerStartTime = DateTime.UtcNow;
    [SerializeField] string saveID = "uniqueTimerSaveID";
    [SerializeField] public TMP_Text TimeText;
    [SerializeField] public RectTransform TimePanel;
    [SerializeField] public RectTransform ClockImage;

    
    private StringBuilder sb;
    private SimpleLongSave save;
    [SerializeField] public bool TimerDebug = false;


    public void Initializer()
    {
        TimerDebug = false;
        TimeText.gameObject.SetActive(false);
        TimePanel.gameObject.SetActive(false);
        ClockImage.gameObject.SetActive(false);
        string timerData = PlayerPrefs.GetString($"TimerProduct_{saveID}", DateTime.UtcNow.Date.AddDays(1).ToBinary().ToString());
        timerStartTime = DateTime.FromBinary(long.Parse(timerData));
        sb = new StringBuilder();

    }
    public void TimerConstant()
    {
        if (TimerDebug == true)
        {
            ResetTimerDebug();
            TimerDebug = false;
        }

        if (uIWheelSpin == null)
        {
            Debug.LogWarning("[Timer] UIWheelSpin reference is missing.");
            return;
        }

        if (TimeText == null)
        {
            Debug.LogWarning("[Timer] TimeText reference is missing.");
            return;
        }

        DateTime now = DateTime.UtcNow;
        TimeSpan timeRemaining = timerStartTime - now;

        if (now >= timerStartTime || uIWheelSpin.isSpinning == false)
        {
            uIWheelSpin.isSpinning = false;
            uIWheelSpin.spinningButton.interactable = true;
            TimeText.gameObject.SetActive(false);
            TimePanel.gameObject.SetActive(false);
            ClockImage.gameObject.SetActive(false);
        }
        else
        {
            uIWheelSpin.spinningButton.interactable = false;
            TimeText.gameObject.SetActive(true);
            TimePanel.gameObject.SetActive(true);
            ClockImage.gameObject.SetActive(true);
            uIWheelSpin.spinningButton.interactable = false;
            TimeText.text = FormatTimer(timeRemaining);

        }
    }

    public void StartTimer(bool skipCooldown = false)
    {
        timerStartTime = DateTime.UtcNow.Date.AddDays(1);

        PlayerPrefs.SetString($"TimerProduct_{saveID}", timerStartTime.ToBinary().ToString());
        PlayerPrefs.Save();
        ShowTimeText();
    }

    public void ShowTimeText()
    {
        TimeText.gameObject.SetActive(true);
        Debug.Log("TimeText SpinCooldown shown");
    }

    private string FormatTimer(TimeSpan timeSpan)
    {
        if (sb == null)
        {
            sb = new StringBuilder();
        }

        sb.Clear();

        if (timeSpan.Hours > 0)
        {
            sb.Append(timeSpan.Hours);
            sb.Append(':');
        }

        sb.Append(timeSpan.Minutes.ToString("00"));
        sb.Append(':');

        sb.Append(timeSpan.Seconds.ToString("00"));

        return sb.ToString();
    }
    public bool IsAvailable()
    {
        return DateTime.UtcNow >= timerStartTime;
    }

#if UNITY_EDITOR
    [ContextMenu("Reset Timer (Debug)")]
    public void ResetTimerDebug()
    {
        timerStartTime = DateTime.UtcNow.Date.AddDays(1);
        PlayerPrefs.SetString($"TimerProduct_{saveID}", timerStartTime.ToBinary().ToString());
        PlayerPrefs.Save();

        if (uIWheelSpin != null)
        {
            uIWheelSpin.isSpinning = false;
            uIWheelSpin.spinningButton.interactable = true;
            if (TimeText != null)
            {
                TimeText.gameObject.SetActive(false);
            }
        }

        Debug.Log("Timer reset for debugging purposes.");
    }
#endif
}

