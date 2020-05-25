using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractUIOperator : MonoBehaviour,IChengeUIState
{
    public enum OperateType
    {
        ADD,CLOSE,CLOSETO
    }
    [SerializeField] OperateType _operateType;
    [SerializeField] UIBase _nextUI;
    UIBase _myUIBase;

    

    protected abstract bool OperateTerm();

    private void Start()
    {
        _myUIBase = GetComponent<UIBase>();
        if (_myUIBase == null)
        {
            Debug.Log(string.Format("{0}'s UIOperator is not attached UIBase", gameObject.name));
        }
    }

    private void Update()
    {
        if (OperateTerm())
        {
            switch (_operateType)
            {
                case OperateType.ADD:
                    _myUIBase.AddUI(_nextUI);
                    break;
                case OperateType.CLOSE:
                    _myUIBase.CloseUI(_nextUI);
                    break;
                case OperateType.CLOSETO:
                    _myUIBase.CloseToUI(_nextUI);
                    break;
            }

        }
    }

    void IChengeUIState.RecieveChenge(UIBase.UIState changed)
    {
        switch (changed)
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
}
