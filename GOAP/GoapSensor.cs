using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoapSensor
{
    public void AddData(GoapMemory memory, string key, object value)
    {
        memory.AddData(key, value);
    }

    public void AddWorldObjectsData(GoapMemory memory, string key, string objectType)
    {
        memory.AddWorldData(key, WorldData.GetData(objectType));
    }
}
