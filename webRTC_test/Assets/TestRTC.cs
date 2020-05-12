using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;

public class TestRTC : MonoBehaviour
{
    [SerializeField] Text sendText;
    [SerializeField] Text recieveText;

    RTCPeerConnection localConnection;
    RTCPeerConnection remoteConnection;

    private RTCDataChannel dataChannel, remoteDataChannel;

    private RTCOfferOptions OfferOptions = new RTCOfferOptions
    {
        iceRestart = false,
        offerToReceiveAudio = true,
        offerToReceiveVideo = false
    };
    private RTCAnswerOptions AnswerOptions = new RTCAnswerOptions
    {
        iceRestart = false,
    };
    private DelegateOnMessage onDataChannelMessage;

    private void Awake()
    {
        WebRTC.Initialize();
        //メッセージ受信時の処理
        onDataChannelMessage = new DelegateOnMessage(bytes => { recieveText.text = System.Text.Encoding.UTF8.GetString(bytes); });
    }


    private void OnDestroy()
    {
        WebRTC.Finalize();
    }

    //ピアの接続をする
    IEnumerator CreatePeer()
    {
        //ローカル
        localConnection = new RTCPeerConnection();
        RTCDataChannelInit conf = new RTCDataChannelInit(true);
        dataChannel= localConnection.CreateDataChannel("send",ref conf);
        dataChannel.OnOpen = new DelegateOnOpen(() => { Debug.Log("localOpen"); });
        dataChannel.OnClose = new DelegateOnClose(() => { Debug.Log("localClose"); });
        localConnection.OnIceConnectionChange = 
            new DelegateOnIceConnectionChange(state=> { OnIceConnectionChange(localConnection, state); });

        //リモート
        remoteConnection = new RTCPeerConnection();
        remoteConnection.OnDataChannel = new DelegateOnDataChannel(
            x =>
            {
                remoteDataChannel = x;
                remoteDataChannel.OnMessage = onDataChannelMessage;
            });
        localConnection.OnIceConnectionChange =
            new DelegateOnIceConnectionChange(state => { OnIceConnectionChange(remoteConnection, state); });

        //ICEの登録
        localConnection.OnIceCandidate = new DelegateOnIceCandidate(candidate => { remoteConnection.AddIceCandidate(ref candidate); });
        remoteConnection.OnIceCandidate = new DelegateOnIceCandidate(candidate => { localConnection.AddIceCandidate(ref candidate); });

        //SDPの交換
        //オファー
        var op1 = localConnection.CreateOffer(ref OfferOptions);
        yield return op1;
        var op2 = localConnection.SetLocalDescription(ref op1.desc);
        yield return op2;
        var op3 = remoteConnection.SetRemoteDescription(ref op1.desc);
        yield return op3;
        //アンサー
        var op4 = remoteConnection.CreateAnswer(ref AnswerOptions);
        yield return op4;
        var op5 = remoteConnection.SetLocalDescription(ref op4.desc);
        yield return op5;
        var op6 = localConnection.SetRemoteDescription(ref op4.desc);
        yield return op6;
    }

    public void OnclickCreatePeer()
    {
        StartCoroutine(CreatePeer());
    }

    public void SendMsg()
    {
        dataChannel.Send(sendText.text);
    }

    //裏方======================================
    void OnIceConnectionChange(RTCPeerConnection pc, RTCIceConnectionState state)
    {
        switch (state)
        {
            case RTCIceConnectionState.New:
                Debug.Log($"{GetName(pc)} IceConnectionState: New");
                break;
            case RTCIceConnectionState.Checking:
                Debug.Log($"{GetName(pc)} IceConnectionState: Checking");
                break;
            case RTCIceConnectionState.Closed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Closed");
                break;
            case RTCIceConnectionState.Completed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Completed");
                break;
            case RTCIceConnectionState.Connected:
                Debug.Log($"{GetName(pc)} IceConnectionState: Connected");
                break;
            case RTCIceConnectionState.Disconnected:
                Debug.Log($"{GetName(pc)} IceConnectionState: Disconnected");
                break;
            case RTCIceConnectionState.Failed:
                Debug.Log($"{GetName(pc)} IceConnectionState: Failed");
                break;
            case RTCIceConnectionState.Max:
                Debug.Log($"{GetName(pc)} IceConnectionState: Max");
                break;
            default:
                break;
        }
    }
    //=======================================
    RTCPeerConnection GetOtherPc(RTCPeerConnection pc)
    {
        return (pc == localConnection) ? remoteConnection : localConnection;
    }

    string GetName(RTCPeerConnection pc)
    {
        return (pc == localConnection) ? "pc1" : "pc2";
    }
}
