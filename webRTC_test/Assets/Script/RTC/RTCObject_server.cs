using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.WebRTC;
using UnityEngine.UI;
using System.Linq;
using NCMB;

public class RTCObject_server : MonoBehaviour
{
    public enum RTCTYPE
    {
        OFFER, ANSWER
    }
    [SerializeField] RTCTYPE _rtcType;
    [SerializeField] string _serchId;
    string _objectID;

    [SerializeField] Text sendText;
    [SerializeField] Text recieveText;
    //[SerializeField] Text _mySDPText;
    //[SerializeField] Text _recieveSDPText;

    RTCPeerConnection localConnection;

    private RTCDataChannel localDataChannel;
    private RTCDataChannel remoteDataChannel;

    List<RTCIceCandidate> _localIceCandidate = new List<RTCIceCandidate>();
    //[SerializeField] TestRTC2 _connectTarget;
    

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

        _serchId = StringUtils.GeneratePassword(8);
    }


    private void OnDestroy()
    {
        WebRTC.Finalize();
    }
    #region peerAction_raw
    //ピアの作成をする
    void CreatePeer()
    {
        //ローカル
        RTCConfiguration pc_config = new RTCConfiguration();
        var server = new RTCIceServer();
        server.urls = new string[] { "stun:stun.webrtc.ecl.ntt.com:3478" };
        pc_config.iceServers = new RTCIceServer[] {
            server
        };
        localConnection = new RTCPeerConnection(ref pc_config);
        RTCDataChannelInit conf = new RTCDataChannelInit(true);
        localDataChannel = localConnection.CreateDataChannel("send", ref conf);
        localDataChannel.OnOpen = new DelegateOnOpen(() => { Debug.Log("localOpen"); });
        localDataChannel.OnClose = new DelegateOnClose(() => { Debug.Log("localClose"); });
        //localDataChannel.OnMessage = onDataChannelMessage;
        localConnection.OnDataChannel = new DelegateOnDataChannel(x => {
            Debug.Log("ondatachannel");
            remoteDataChannel = x;
            remoteDataChannel.OnMessage = onDataChannelMessage;
        });
        localConnection.OnIceConnectionChange =
            new DelegateOnIceConnectionChange(state => { OnIceConnectionChange(localConnection, state); });

        //ICEの登録
        localConnection.OnIceCandidate = new DelegateOnIceCandidate(candidate => {
            if (!string.IsNullOrEmpty(candidate.candidate))
            {
                _localIceCandidate.Add(candidate);
                Debug.Log("add ice from local to remote" + candidate.candidate);
            }
            else
            {
                Debug.Log("end ice candidate");
            }
        });

        Debug.Log("crete peer");
    }

    //RTCSessionDescription ConvertText2desc(Text target, bool isOffer)
    //{
    //    var sdp = target.text;
    //    var result = new RTCSessionDescription();
    //    result.type = (isOffer) ? RTCSdpType.Offer : RTCSdpType.Answer;
    //    result.sdp = sdp;
    //    return result;
    //}

    void SendSDP(RTCSessionDescription session)
    {
        var type = (session.type == RTCSdpType.Offer) ? RTCSendData.DATATYPE.OFFERE : RTCSendData.DATATYPE.ANSWER;
        var data = new RTCSendData(type,session.sdp);
        var json = ToJson(data);
        var obj= NCMB_RTC.CreateObject(_serchId,json);
        obj.SaveAsync();
    }

    RTCSessionDescription RecieveSDP()
    {
        NCMB_RTC.GetCount();
        var obj = NCMB_RTC.GetObject(_serchId);
        string json = obj["json"].ToString();
        var data = FromJson<RTCSendData>(json);
        bool isOffer = (data._datatype == RTCSendData.DATATYPE.OFFERE) ? true : false;

        var result = new RTCSessionDescription();
        result.type = (isOffer) ? RTCSdpType.Offer : RTCSdpType.Answer;
        result.sdp = data._sdp;
        return result;
    }

    //void TextSDP(RTCSessionDescription session, Text text)
    //{
    //    try
    //    {
    //        text.text = session.sdp;
    //        Debug.Log(text.text);
    //    }
    //    catch
    //    {
    //        Debug.Log("miss get desc");
    //    }
    //}
    IEnumerator CreateOffer()
    {
        if (_rtcType != RTCTYPE.OFFER) yield break;
        //オファー
        var op1 = localConnection.CreateOffer(ref OfferOptions);
        yield return op1;
        var op2 = localConnection.SetLocalDescription(ref op1.desc);
        yield return op2;
        if (!op1.isError)
        {
            //TextSDP(op1.desc, _mySDPText);
            SendSDP(op1.desc);
        }
        else
        {
            Debug.Log("error offer");
        }
    }
    void RecieveOffer()
    {
        if (_rtcType != RTCTYPE.ANSWER) return;
        //var offer = ConvertText2desc(_recieveSDPText, true);
        var offer = RecieveSDP();
        try
        {
            Debug.Log($"recieve offer");
            localConnection.SetRemoteDescription(ref offer);
            StartCoroutine(CreateAnswer());
        }
        catch
        {
            Debug.Log("cant offer recieve");
        }
    }

    IEnumerator CreateAnswer()
    {
        if (_rtcType != RTCTYPE.ANSWER) yield break;
        //アンサー
        var op4 = localConnection.CreateAnswer(ref AnswerOptions);
        yield return op4;
        var op5 = localConnection.SetLocalDescription(ref op4.desc);
        yield return op5;
        //TextSDP(op4.desc, _mySDPText);
        SendSDP(op4.desc);
        Debug.Log("create answer");
    }

    void RecieveAnswer()
    {
        if (_rtcType != RTCTYPE.OFFER) return;
        //var answer = ConvertText2desc(_recieveSDPText, false);
        var answer = RecieveSDP();
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

    void SendIceCandidata(List<string> jsonlist)
    {
        var obj = NCMB_RTC.GetObject(_serchId);
        var json = obj["json"].ToString();
        var data = FromJson<RTCSendData>(json);
        if(_rtcType== RTCTYPE.OFFER)
        {
            data._candidates_offer = jsonlist;
        }
        else
        {
            data._candidates_answer = jsonlist;
        }
        json = ToJson(data);
        obj["json"] = json;
        obj.SaveAsync();

        //_connectTarget.RecieveIceCandidate(jsonlist);
        Debug.Log($"sendJson");
    }

    void RecieveIceCandidate()
    {
        var obj = NCMB_RTC.GetObject(_serchId);
        var json = obj["json"].ToString();
        var data = FromJson<RTCSendData>(json);
        var targetList = (_rtcType == RTCTYPE.OFFER) ? data._candidates_answer : data._candidates_offer;
        List<RTCIceCandidate> remoteICE = new List<RTCIceCandidate>();
        foreach (var target in targetList)
        {
            var d = FromJson<RTCIceCandidate>(target);
            remoteICE.Add(d);
        }

        remoteICE.ForEach(x => localConnection.AddIceCandidate(ref x));
        Debug.Log($"recieveJson {gameObject.name}");
    }
    #endregion

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

    public void SendIce_onclick()
    {
        var jsonlist = _localIceCandidate.Select(x => ToJson(x)).ToList();
        SendIceCandidata(jsonlist);
    }

    public void RecieveIce_onclick()
    {
        RecieveIceCandidate();
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


    string GetName(RTCPeerConnection pc)
    {
        return (pc == localConnection) ? "localConnection" : "remoteConnection";
    }

    string ToJson<T>(T data)
    {
        var json = JsonUtility.ToJson(data);
        return json;
    }

    T FromJson<T>(string json)
    {
        var data = JsonUtility.FromJson<T>(json);
        return data;
    }

}
