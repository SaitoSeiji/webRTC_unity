using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChengeFlag 
{
    Action ChengeTrueAction;
    Action ChengeFalseAction;
    bool beforeFlag;
    public bool nowFlag { get; private set; }

    public void SetAction(bool flag,Action act)
    {
        if (flag) ChengeTrueAction = act;
        else ChengeFalseAction =act;
    }

    public void SetFlag(bool target)
    {
        nowFlag = target;
    }

    public void CheckFlag(bool target)
    {
        beforeFlag = nowFlag;
        nowFlag = target;
        if (beforeFlag != nowFlag)
        {
            if (target)
            {
                ChengeTrueAction.Invoke();
            }
            else
            {
                ChengeFalseAction.Invoke();
            }
        }
    }
}
