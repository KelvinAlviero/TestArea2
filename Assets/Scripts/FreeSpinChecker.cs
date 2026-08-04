using UnityEngine;

public class FreeSpinChecker : MonoBehaviour
{
    [Header("Script References")]
    [SerializeField] private APIController APIController;
    [SerializeField] private SpinningScript SpinningScript;
    [SerializeField] private WheelPopulate wheelPopulate;
    [SerializeField] private FreeSpinChecker freeSpinChecker;
    [SerializeField] public UIWheelSpin uIWheelSpin;
    [SerializeField] private Timer timer;


    public bool FreeSpinAvailable
    {
        get
        {
            return uIWheelSpin.freeSpinButton;
        } 
        set
        {
            uIWheelSpin.freeSpinButton = value;
        }
    }

    public void CheckPaidSpinOnFree()
    {
        if (FreeSpinAvailable == true)
        {
            uIWheelSpin.DisableSpinPaidButton();
        }
        else
        {
            uIWheelSpin.EnableSpinPaidButton();  
        }
    }

    

    public void FreeSpinCheck()
    {
        if (FreeSpinAvailable == false)
        {
            uIWheelSpin.DisableSpinButton();
            timer.StartTimer();
            Debug.Log("FreeSpinDisabled");
        }
        else
        {
            uIWheelSpin.EnableSpinButton();
        }
    }
}
