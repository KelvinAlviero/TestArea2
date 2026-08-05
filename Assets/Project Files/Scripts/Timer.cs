using System;
using System.Text;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public UIWheelSpin uIWheelSpin;
    public FreeSpinChecker freeSpinChecker;

    [SerializeField] private DateTime timerStartTime = DateTime.UtcNow;
    [SerializeField] public TMP_Text TimeText;
    [SerializeField] public RectTransform TimePanel;
    [SerializeField] public RectTransform ClockImage;
    [SerializeField] private float SpinEndTimer;
    private StringBuilder sb;
    [SerializeField] public bool TimerDebug = false;

    public float GetSpinEndTimer()
    {
        return SpinEndTimer;
    }

    public void SetSpinEndTimer(float value)
    {
        SpinEndTimer = value;
    }

    public void Initializer()
    {
        TimerDebug = false;
        HideTimeText();
        // Session-only; free-spin availability comes from the backend.
        timerStartTime = DateTime.UtcNow;
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

        if (freeSpinChecker.FreeSpinAvailable)
        {
            HideTimeText();
            return;
        }

        if (now >= timerStartTime)
        {
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
        freeSpinChecker.FreeSpinAvailable = true;

        if (uIWheelSpin != null)
        {
            uIWheelSpin.SetIsSpinning(false);
            uIWheelSpin.spinFree.interactable = freeSpinChecker.FreeSpinAvailable;
            freeSpinChecker.FreeSpinAvailable = true;
            HideTimeText();
        }

        Debug.Log("Timer reset for debugging purposes.");
    }
}
