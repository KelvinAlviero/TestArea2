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
        HideTimeText();
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

        if (now >= timerStartTime)
        {
            //Free spin ready
            uIWheelSpin.isSpinning = false;
            uIWheelSpin.FreeSpinAvailable = true;
            uIWheelSpin.spinFree.interactable = uIWheelSpin.FreeSpinAvailable;
            HideTimeText();
        }
        else
        {
            //free spin not ready
            uIWheelSpin.spinFree.interactable = false;
            ShowTimeText();
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
        if (TimeText != null)
            TimeText.gameObject.SetActive(true);
        if (TimePanel != null)
            TimePanel.gameObject.SetActive(true);
        if (ClockImage != null)
            ClockImage.gameObject.SetActive(true);
    }

    private void HideTimeText()
    {
        if (TimeText != null)
            TimeText.gameObject.SetActive(false);
        if (TimePanel != null)
            TimePanel.gameObject.SetActive(false);
        if (ClockImage != null)
            ClockImage.gameObject.SetActive(false);
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

    [ContextMenu("Reset Timer (Debug)")]
    public void ResetTimerDebug()
    {

        timerStartTime = DateTime.UtcNow.AddSeconds(-1);
        PlayerPrefs.SetString($"TimerProduct_{saveID}", timerStartTime.ToBinary().ToString());
        PlayerPrefs.Save();
        uIWheelSpin.FreeSpinAvailable = true;


        if (uIWheelSpin != null)
        {
            uIWheelSpin.isSpinning = false;
            uIWheelSpin.spinFree.interactable = uIWheelSpin.FreeSpinAvailable;
            uIWheelSpin.FreeSpinAvailable = true;
            HideTimeText();
        }

        Debug.Log("Timer reset for debugging purposes.");
    }
}

