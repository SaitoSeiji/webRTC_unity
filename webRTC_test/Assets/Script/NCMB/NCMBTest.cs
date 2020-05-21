using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCMB;

public class NCMBTest : MonoBehaviour
{
    [SerializeField] string serchId;
    [SerializeField] string message;

    NCMBObject myObject;

    private void Start()
    {
        
    }
    
    [ContextMenu("addTestClass")]
    void AddTestClass()
    {

        myObject = new NCMBObject("TestClass");
        myObject["message"] = message;
        myObject["id"] = serchId;
        myObject.SaveAsync((NCMBException e) => {
            if (e != null)
            {

            }
            else
            {
                LogObj();
            }
        });
    }

    [ContextMenu("logObj")]
    void LogObj()
    {
        Debug.Log(myObject.ObjectId);
    }

    [ContextMenu("LoadTestClass")]
    void LoadTestClassData()
    {
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("TestClass");
        query.WhereEqualTo("id",serchId);
        query.FindAsync((List<NCMBObject> objlist,NCMBException e)=> {
            if (e !=null)
            {
                Debug.Log("error");
            }
            else
            {
                foreach(var obj in objlist)
                {
                    Debug.Log($"id is {obj.ObjectId}");
                    var message=obj["message"];
                    Debug.Log($"message is {message}");
                }
            }
        });
    }
    [ContextMenu("GetCount")]
    void GetCount()
    {
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>("TestClass");
        query.CountAsync((int count, NCMBException e) => {
            if (e != null)
            {
                Debug.Log("error");
            }
            else
            {
                Debug.Log($"件数：{count}");
            }
        });
    }
}
