using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitFlag 
{
    float _waitLength;
    float _waitStartTime;

    public bool _endSetUp { get; private set; }

    public WaitFlag()
    {
        _endSetUp = false;
    }
    

    public bool _waitNow
    {
        get
        {
            return Time.fixedTime < _waitLength + _waitStartTime;
        }
    }

    public void WaitStart()
    {
        _waitStartTime = Time.fixedTime;
    }
    

    public void SetWaitLength(float wait)
    {
        _waitLength = wait;
        _waitStartTime = -wait;
        _endSetUp = true;
    }
}
