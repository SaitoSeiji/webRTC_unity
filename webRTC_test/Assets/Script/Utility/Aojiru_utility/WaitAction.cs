using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaitAction:SingletonMonoBehaviour<WaitAction>
{
    public void CoalWaitAction(UnityAction ua, float time)
    {
        StartCoroutine(WaitCorutin(ua, time));
    }
    public void CoalWaitAction_frame(UnityAction ua, int frame)
    {
        StartCoroutine(WaitCorutin(ua, frame));
    }

    IEnumerator WaitCorutin(UnityAction ua, float time)
    {
        yield return new WaitForSeconds(time);
        ua.Invoke();
    }
    IEnumerator WaitCorutin(UnityAction ua, int frame)
    {
        while (frame > 0)
        {
            frame--;
            yield return null;
        }
        ua.Invoke();
    }
}
