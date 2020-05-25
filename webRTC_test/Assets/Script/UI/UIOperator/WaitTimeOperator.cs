using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitTimeOperator : AbstractUIOperator
{

    [SerializeField] float _waitTime;
    WaitFlag _waitFlag;


    protected override void ChengeState_toActive()
    {
        base.ChengeState_toActive();
        _waitFlag = new WaitFlag();
        _waitFlag.SetWaitLength(_waitTime);
        _waitFlag.WaitStart();
    }

    protected override void ChengeState_toClose()
    {
        base.ChengeState_toClose();
        _waitFlag = null;
    }

    protected override void ChengeState_toSleep()
    {
        base.ChengeState_toSleep();
        _waitFlag = null;
    }

    protected override bool OperateTerm()
    {
        if (_waitFlag == null)
        {
            return false;
        }

        if (!_waitFlag._waitNow)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
