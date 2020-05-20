using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCMB;

//サーバーに送るデータ
[System.Serializable]
public class RTCSendData
{
    public enum DATATYPE
    {
        OFFERE,ANSWER
    }

    public DATATYPE _datatype;
    public string _sdp;
    public List<string> _candidates_offer = new List<string>();
    public List<string> _candidates_answer = new List<string>();
    public RTCSendData(DATATYPE datatype,string sdp)
    {
        _sdp = sdp;
        _datatype = datatype;
    }
}

public class NCMB_RTC
{
    public static NCMBObject CreateObject(string serchID,string json)
    {
        NCMBObject myclass = new NCMBObject("NCMB_RTC");
        myclass["_serchID"]=serchID;
        myclass["json"]=json;
        return myclass;
    }

    public static NCMBObject GetObject(string serchID)
    {
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("NCMB_RTC");
        query.WhereEqualTo("_serchID", serchID);
        NCMBObject tempObj = null;
        query.FindAsync((List<NCMBObject> objlist, NCMBException e) => {
            if (e != null)
            {
                Debug.Log("error");
            }
            else
            {
                foreach (var obj in objlist)
                {
                    tempObj = obj;
                }
            }
        });
        return tempObj;
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