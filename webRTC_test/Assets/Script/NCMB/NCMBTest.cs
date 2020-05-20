using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCMB;

public class NCMBTest : MonoBehaviour
{
    [SerializeField] string serchId;
    [SerializeField] string message;
    
    private void Start()
    {
        
    }

    [ContextMenu("addTestClass")]
    void AddTestClass()
    {
        NCMBObject testclass = new NCMBObject("TestClass");
        testclass["message"] = message;
        testclass["id"] = serchId;
        testclass.SaveAsync();
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
