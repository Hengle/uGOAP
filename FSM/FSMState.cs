using System.Collections;
using UnityEngine;

public interface FSMState 
{
    void Update (FSM fsm, GameObject gameObject);
}
