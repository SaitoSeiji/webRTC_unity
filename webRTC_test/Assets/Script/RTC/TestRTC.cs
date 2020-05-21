using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;

public class TestRTC : MonoBehaviour
{
    [SerializeField] Text sendText;
    [SerializeField] Text recieveText;
    [SerializeField] Text _offerSDPText;
    [SerializeField] Text _answerSDPText;

    RTCPeerConnection localConnection;
    RTCPeerConnection remoteConnection;

    private RTCDataChannel localDataChannel, remoteDataChannel;

    List<RTCIceCandidate> _localIceCandidate=new List<RTCIceCandidate>();
    List<RTCIceCandidate> _remoteIceCandidate=new List<RTCIceCandidate>();

    private RTCOfferOptions OfferOptions = new RTCOfferOptions
    {
        iceRestart = true,
        offerToReceiveAudio = true,
        offerToReceiveVideo = false
    };
    private RTCAnswerOptions AnswerOptions = new RTCAnswerOptions
    {
        iceRestart = true,
    };
    private DelegateOnMessage onDataChannelMessage;//データチャンネル受信時のコールバック

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

    //ピアの作成をする
    void CreatePeer()
    {
        //ローカル
        RTCConfiguration pc_config=new RTCConfiguration();
        var server = new RTCIceServer();
        server.urls =new string[] { "stun:stun.webrtc.ecl.ntt.com:3478" };
        pc_config.iceServers =new RTCIceServer[] {
            server
        };
        localConnection = new RTCPeerConnection(ref pc_config);
        RTCDataChannelInit conf = new RTCDataChannelInit(true);
        localDataChannel= localConnection.CreateDataChannel("send",ref conf);
        localDataChannel.OnOpen = new DelegateOnOpen(() => { Debug.Log("localOpen"); });
        localDataChannel.OnClose = new DelegateOnClose(() => { Debug.Log("localClose"); });
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
        localConnection.OnIceCandidate = new DelegateOnIceCandidate(candidate => {
            if (!string.IsNullOrEmpty(candidate.candidate))
            {
                _localIceCandidate.Add(candidate);
                //remoteConnection.AddIceCandidate(ref candidate);
                Debug.Log("add ice from local to remote" + candidate.candidate);
            }
            else
            {
                Debug.Log("end ice candidate");
            }
        });
        remoteConnection.OnIceCandidate = new DelegateOnIceCandidate(candidate =>
        {
            if (!string.IsNullOrEmpty(candidate.candidate))
            {
                _remoteIceCandidate.Add(candidate);
                //localConnection.AddIceCandidate(ref candidate);
                Debug.Log("add ice from remote to local:" + candidate.candidate);
            }
            else
            {
                Debug.Log("end ice candidate");
            }
        });

        Debug.Log("crete peer");
    }

    RTCSessionDescription ConvertText2desc(Text target, bool isOffer)
    {
        var sdp = target.text;
        var result = new RTCSessionDescription();
        result.type = (isOffer) ? RTCSdpType.Offer : RTCSdpType.Answer;
        result.sdp = sdp;
        return result;
    }
    void TextSDP(RTCSessionDescription session,Text text)
    {
        try
        {
            text.text = session.sdp;
            Debug.Log(text.text);
        }
        catch
        {
            Debug.Log("miss get desc");
        }
    }
    IEnumerator CreateOffer()
    {
        //オファー
        var op1 = localConnection.CreateOffer(ref OfferOptions);
        yield return op1;
        var op2 = localConnection.SetLocalDescription(ref op1.desc);
        yield return op2;
        if (!op1.isError)
        {
            TextSDP(op1.desc,_offerSDPText);
        }
        else
        {
            Debug.Log("error offer");
        }
    }
    void RecieveOffer()
    {
        var offer = ConvertText2desc(_offerSDPText, true);
        try
        {
            Debug.Log($"recieve offer:{_offerSDPText.text}");
            remoteConnection.SetRemoteDescription(ref offer);
            StartCoroutine(CreateAnswer());
        }
        catch
        {
            Debug.Log("cant offer recieve");
        }
    }

    IEnumerator CreateAnswer()
    {
        //アンサー
        var op4 = remoteConnection.CreateAnswer(ref AnswerOptions);
        yield return op4;
        var op5 = remoteConnection.SetLocalDescription(ref op4.desc);
        yield return op5;
        TextSDP(op4.desc,_answerSDPText);
        Debug.Log("create answer");
    }

    void RecieveAnswer()
    {
        var answer = ConvertText2desc(_answerSDPText, false);
        try
        {
            localConnection.SetRemoteDescription(ref answer);
            Debug.Log("recieved answere");

        }
        catch
        {
            Debug.Log("cant answer recieve");
        }
    }

    public void ExchengeIceCandiate()
    {
        _localIceCandidate.ForEach(x => remoteConnection.AddIceCandidate(ref x));
        _remoteIceCandidate.ForEach(x => localConnection.AddIceCandidate(ref x));
    }

    public void OnclickCreatePeer()
    {
        CreatePeer();
    }

    public void OnclickCreateOffer()
    {
        StartCoroutine(CreateOffer());
    }

    public void OnclickRecieveOffer()
    {
        RecieveOffer();
    }
    public void OnclickRecieveAnswer()
    {
        RecieveAnswer();
    }

    public void SendMsg()
    {
        localDataChannel.Send(sendText.text);
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
        return (pc == localConnection) ? "localConnection" : "remoteConnection";
    }
}
