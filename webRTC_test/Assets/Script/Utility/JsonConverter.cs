using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JsonConverter
{
    public static string ToJson<T>(T data)
    {
        var json = JsonUtility.ToJson(data);
        return json;
    }

    public static T FromJson<T>(string json)
    {
        var data = JsonUtility.FromJson<T>(json);
        return data;
    }
}
