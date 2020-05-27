using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCMB;
using System;
using System.Threading.Tasks;

//サーバーに送るデータ jsonにして送る
[System.Serializable]
public class RTCSendData
{
    public enum DATATYPE
    {
        OFFERE,ANSWER
    }

    public DATATYPE _datatype;
    public string _sdp;
    public List<string> candidateJson = new List<string>();
    public RTCSendData(DATATYPE datatype,string sdp)
    {
        _sdp = sdp;
        _datatype = datatype;
    }
}

[System.Serializable]
public class NCMBStateData
{
    public enum MyNCMBstate
    {
        NONE,
        CREATEDROOM,//ホストが部屋を作成した
        SELECTEDROOM,//子が部屋を選択してマッチングが成立した
        SENDED_offer,//ホストがofferを送った
        SENDED_answer,//子がanswerを送った
        CONNECTED_sdp,//ホストがanswerを受け取りsdpの交換が終了した
        SENDED_ice,//ICEの送信が完了した
        CONNECTED//ICEの受信が完了し、接続が開始した
    }
    public MyNCMBstate myNCMBstate;

    public NCMBStateData(MyNCMBstate state)
    {
        myNCMBstate = state;
    }
}

[System.Serializable]
public class NCMB_RTC
{
    string _objectID="";
    public bool _created { get; private set; } = false;
    bool _actNow = false;
    bool _delete = false;

    //自分の状態
    [SerializeField] NCMBStateData.MyNCMBstate _myNCMBState;
    public NCMBStateData.MyNCMBstate _MyNCMBState { get { return _myNCMBState; } }

    //接続先の状態
    [SerializeField] NCMBStateData.MyNCMBstate _serverNCMBState;
    public NCMBStateData.MyNCMBstate _ServerNCMBState { get { return _serverNCMBState; } }

    public bool _IsStateChenged
    {
        get
        {
            return _serverNCMBState>_myNCMBState;
        }
    }

    static string _objKey_serchID = "serchID";
    static string _objKey_hostName = "hostName";
    static string _objKey_json_offer = "json_offer";
    static string _objKey_json_answer = "json_answer";
    static string _objKey_json_connectState = "connectState";

    //myclass["serchID"]
    //myclass["hostName"]
    //myclass["json_offer"]
    //myclass["json_answer"]
    //["connectState"]

    public NCMBObject CreateObject(string serchID,string hostName)
    {
        NCMBObject myclass = new NCMBObject("NCMB_RTC");
        myclass[_objKey_serchID] = serchID;
        myclass[_objKey_hostName] = hostName;
        return myclass;
    }

    public void SaveObject(NCMBObject obj)
    {
        if (_created)
        {
            Debug.LogWarning("you can only one object save");
            return;
        }

        if (_actNow)
        {
            Debug.LogWarning("now saving action:you cant additional save");
            return;
        }
        _actNow = true;
        obj.SaveAsync((NCMBException e) =>
        {
            if (e != null)
            {

            }
            else
            {
                _created = true;
                _objectID = obj.ObjectId;
                
                var json = GetJson_connectState(obj);
                _myNCMBState = JsonConverter.FromJson<NCMBStateData>(json).myNCMBstate;
                _serverNCMBState = _myNCMBState;

                Debug.Log("save end");
            }
        });
    }

    public void FetchObject(Action<NCMBObject> act)
    {
        if (!_created) return;
        var obj = new NCMBObject("NCMB_RTC");
        obj.ObjectId = _objectID;
        obj.FetchAsync((NCMBException e)=> {
            if (e != null)
            {

            }
            else
            {
                var json= GetJson_connectState(obj);
                _serverNCMBState = JsonConverter.FromJson<NCMBStateData>(json).myNCMBstate;
                act?.Invoke(obj);
            }
        });
    }

    public void UpdateObject(NCMBObject obj)
    {
        if (_objectID != obj.ObjectId)
        {
            Debug.LogWarning($"objectid is uncorrect:{_objectID},{obj.ObjectId}");
            return;
        }
        obj.SaveAsync((NCMBException e) =>
        {
            if (e != null)
            {

            }
            else
            {

                var json = GetJson_connectState(obj);
                _myNCMBState = JsonConverter.FromJson<NCMBStateData>(json).myNCMBstate;
                _serverNCMBState = _myNCMBState;
                Debug.Log("update end");
            }
        });
    }

    public void DeleteObject()
    {
        if (!_created) return;
        if (_delete) return;
        _delete = true;
        var obj = new NCMBObject("NCMB_RTC");
        obj.ObjectId = _objectID;
        obj.DeleteAsync((NCMBException e) =>
        {
            if (e != null)
            {

            }
            else
            {
                Debug.Log("delete end");
            }
        });
    }

    public void CheckStateUpdate()
    {
        FetchObject(null);
    }
    #region static
    public static NCMBObject SetJson_SDPData( NCMBObject obj,bool isoffer,string json)
    {
        if (isoffer)
        {
            obj[_objKey_json_offer] = json;
        }
        else
        {
            obj[_objKey_json_answer] = json;
        }
        return obj;
    }

    public static string GetJson_SDPData(NCMBObject obj, bool isoffer)
    {
        if (isoffer)
        {
            return obj[_objKey_json_answer].ToString();
        }
        else
        {
            return obj[_objKey_json_offer].ToString();
        }
    }

    public static NCMBObject SetJson_connectState(NCMBObject obj, string json)
    {
        obj[_objKey_json_connectState] = json;
        return obj;
    }
    public static string GetJson_connectState(NCMBObject obj)
    {
        return obj[_objKey_json_connectState].ToString();
    }

    public static string Get_hostName(NCMBObject obj)
    {
        return obj[_objKey_hostName].ToString();
    }

    public static void GetObject(string serchID, Action<List<NCMBObject>> act)
    {
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("NCMB_RTC");
        query.WhereEqualTo("serchID", serchID);
        NCMBObject tempObj = null;
        query.FindAsync((List<NCMBObject> objlist, NCMBException e) =>
        {
            if (e != null)
            {
                Debug.Log("error");
            }
            else
            {
                act.Invoke(objlist);
            }
        });
    }
    public static void GetCount()
    {
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("NCMB_RTC");
        query.CountAsync((int count, NCMBException e) =>
        {
            if (e != null)
            {
                //件数取得失敗時の処理
            }
            else
            {
                //件数を出力
                Debug.Log("件数 : " + count);
            }
        });
    }
    #endregion
}

public static class StringUtils
{
    private const string PASSWORD_CHARS =
        "0123456789abcdefghijklmnopqrstuvwxyz";

    public static string GeneratePassword(int length)
    {
        var sb = new System.Text.StringBuilder(length);
        var r = new System.Random();

        for (int i = 0; i < length; i++)
        {
            int pos = r.Next(PASSWORD_CHARS.Length);
            char c = PASSWORD_CHARS[pos];
            sb.Append(c);
        }

        return sb.ToString();
    }
}