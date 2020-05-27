using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckConnectScript : AbstractUIScript_onclick
{

    [SerializeField] MatchingNCMB _myMatchingNCMB;
    [SerializeField] UnityEvent _succsessEvent;

    bool active = false;

    protected override void ChengeState_toActive()
    {
        base.ChengeState_toActive();
        active = true;
    }

    protected override void ChengeState_toSleep()
    {
        base.ChengeState_toSleep();
        active = false;
    }

    protected override void ChengeState_toClose()
    {
        base.ChengeState_toClose();
        active = false;
    }

    public override void OnclickAction()
    {
        if (!active) return;
        if (_myMatchingNCMB._SignalingNCMB._ServerNCMBState >= NCMBStateData.MyNCMBstate.SELECTEDROOM) return;
        _myMatchingNCMB.StateUpdate_onClick();
    }

    private void Update()
    {
        if (!active) return;
        if(_myMatchingNCMB._SignalingNCMB._ServerNCMBState >= NCMBStateData.MyNCMBstate.SELECTEDROOM)
        {
            _succsessEvent?.Invoke();
        }
    }
}
