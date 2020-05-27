using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectStateOperator : AbstractUIOperator
{
    [SerializeField] MatchingNCMB _myMatching;
    [SerializeField] NCMBStateData.MyNCMBstate _targetState;


    protected override bool OperateTerm()
    {
        if (_myMatching._SignalingNCMB._ServerNCMBState >= _targetState)
        {
            return true;
        }
        return false;
    }
}
