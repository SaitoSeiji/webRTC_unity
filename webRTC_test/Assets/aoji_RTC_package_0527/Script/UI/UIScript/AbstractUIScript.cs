using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//各UIの個別処理を簡単に書くためのスクリプト
//スクリプトという言葉を「簡単な実装をするためのコード」という意味で使っている
public abstract class AbstractUIScript : MonoBehaviour,IChengeUIState
{
    UIBase _myUIBase;
    protected UIBase _MyUIBase
    {
        get
        {
            if (_myUIBase == null) _myUIBase = GetComponent<UIBase>();
            return _myUIBase;
        }
    }
    bool _inited = false;

    private void ChengedUIStateAction(UIBase.UIState chenged)
    {
        switch (chenged)
        {
            case UIBase.UIState.ACTIVE:
                ChengeState_toActive();
                break;
            case UIBase.UIState.SLEEP:
                ChengeState_toSleep();
                break;
            case UIBase.UIState.CLOSE:
                ChengeState_toClose();
                break;
        }
    }

    protected virtual void ChengeState_toActive() { }
    protected virtual void ChengeState_toSleep() { }
    protected virtual void ChengeState_toClose() { }
    protected virtual void InitAction()
    {
        _inited = true;
    }

    void IChengeUIState.RecieveChenge(UIBase.UIState changed)
    {
        if (!_inited) InitAction();
        ChengedUIStateAction(changed);
    }
}
