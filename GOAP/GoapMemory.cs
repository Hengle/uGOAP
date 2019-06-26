using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/**
 * Agent`s memory
 */
public class GoapMemory
{
    private Dictionary<string, object> data = new Dictionary<string, object>();
    private Dictionary<string, List<UnityEngine.Object>> worldData = new Dictionary<string, List<UnityEngine.Object>>();

    public void AddData(string key, object value)
    {
        if (!data.ContainsKey(key))
            data.Add(key, value);
        else
            data[key] = value;
    }

    public object GetData(string key)
    {
        if (data.ContainsKey(key))
        {
            return data[key];
        }
        else { return null; }
    }

    /**
     * Clears old data and adds new
     */
    public void AddWorldData(string key, UnityEngine.Object[] value)
    {
        if (!worldData.ContainsKey(key))
        {
            worldData.Add(key, new List<UnityEngine.Object>());
            worldData[key].AddRange(value);
        }
        else
        {
            worldData[key].Clear();
            worldData[key].AddRange(value);
        }
    }

    public T[] GetWorldData<T>(string key) where T : UnityEngine.Object
    {
        return Array.ConvertAll(worldData[key].ToArray(), item => (T)item);
    }
}
