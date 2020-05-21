using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.WebRTC;

public abstract class AbstractRTC_actioner : MonoBehaviour
{

    [SerializeField]protected Text sendText;
    [SerializeField]protected Text _mySDPText;
    [SerializeField]protected Text _recieveSDPText;

    protected RTCPeerConnection localConnection;
    protected RTCDataChannel localDataChannel;

    protected RTCOfferOptions OfferOptions = new RTCOfferOptions
    {
        iceRestart = true,
        offerToReceiveAudio = true,
        offerToReceiveVideo = false
    };
    protected RTCAnswerOptions AnswerOptions = new RTCAnswerOptions
    {
        iceRestart = true,
    };

    protected abstract RTCPeerConnection CreatePeer();
    protected abstract void RecieveSDPText(RTCPeerConnection peer);

    private void Awake()
    {
        WebRTC.Initialize();
    }
    private void OnDestroy()
    {
        WebRTC.Finalize();
    }


    public void StartConnect()
    {
        localConnection = CreatePeer();

        Debug.Log("startConnect");
    }

    public void RecieveSDP()
    {
        RecieveSDPText(localConnection);
    }

    public void SendSDP_onclick()
    {
        if (localConnection == null) return;
        if (_mySDPText == null) return;

        try
        {
            SDP2Text(localConnection, _mySDPText);
        }
        catch
        {
            Debug.Log("miss get desc");
        }
    }


    protected void SDP2Text(RTCPeerConnection connect, Text text)
    {
        if (text == null) return;
        if (connect == null) return;
        try
        {
            var desc = connect.GetLocalDescription();
            text.text = desc.sdp;
            Debug.Log(text.text);
        }
        catch
        {
            Debug.Log("miss get desc");
        }
    }
}
