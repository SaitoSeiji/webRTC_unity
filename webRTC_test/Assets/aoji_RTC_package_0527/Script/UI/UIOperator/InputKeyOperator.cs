using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputKeyOperator : AbstractUIOperator
{
    public enum InputType
    {
       NONE,SUBMIT,CANCEL
    }
    [SerializeField] InputType _inputType;

    bool CheckIsInput()
    {
        switch (_inputType)
        {
            case InputType.SUBMIT:
                if (Input.GetButtonDown("Submit"))
                {
                    return true;
                }
                break;
            case InputType.CANCEL:
                if (Input.GetButtonDown("Cancel"))
                {
                    return true;
                }
                break;
        }

        return false;
    }

    protected override bool OperateTerm()
    {
        if (CheckIsInput())
        {
            return true;
        }
        return false;
    }
}
