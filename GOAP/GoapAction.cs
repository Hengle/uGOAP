using System.Collections.Generic;
using UnityEngine;

public abstract class GoapAction : MonoBehaviour
{
    private Dictionary<string,object> preconditions;
    public Dictionary<string, object> Preconditions
    {
        get { return preconditions; }
    }

    private Dictionary<string,object> effects;
    public Dictionary<string, object> Effects
    {
        get { return effects; }
    }

    private bool inRange = false;

    protected Animator characterAnimController;

    protected virtual void Awake()
    {
        characterAnimController = gameObject.GetComponent<Animator>();
    }

    /** 
     * The cost of performing the action. 
     * Figure out a weight that suits the action. 
     * Changing it will affect what actions are chosen during planning.
     */
    public float Cost = 1f;
    public virtual float GetCost()
    {
        return Cost;
    }

    protected float startTime = 0f;
    public float duration = 2f;  // seconds to complete the action

    /**
     * An action often has to perform on an object. This is that object. Can be null. 
     */
    [HideInInspector]
    public GameObject target;

    public GoapAction()
    {
        preconditions = new Dictionary<string, object>();
        effects = new Dictionary<string, object>();
    }

    public void DoReset()
    {
        inRange = false;
        target = null;
        startTime = 0;
        reset();
    }

    public virtual Vector3 GetTargetPos()
    {
        return target.transform.position;
    }

    /**
     * Reset any variables that need to be reset before planning happens again.
     */
    public abstract void reset();

    /**
     * Is the action done?
     */
    public abstract bool IsDone();

    /**
     * Procedurally check if this action can run. Not all actions
     * will need this, but some might.
     */
    public abstract bool CheckProceduralPrecondition(GameObject agent, GoapMemory memory);

    /**
     * This function checks the closest path from one action to another
     */
    public virtual bool CheckDistance(GameObject obj, GoapMemory memory) { return false; }

    /**
     * Run the action.
     * Returns True if the action performed successfully or false
     * if something happened and it can no longer perform. In this case
     * the action queue should clear out and the goal cannot be reached.
     */
    public abstract bool Perform(GameObject agent, GoapMemory memory);

    /**
     * Does this action need to be within range of a target game object?
     * If not then the moveTo state will not need to run for this action.
     */
    public abstract bool RequiresInRange();
    

    /**
     * Are we in range of the target?
     * The MoveTo state will set this and it gets reset each time this action is performed.
     */
    public bool IsInRange ()
    {
        return inRange;
    }
    
    public void SetInRange(bool inRange)
    {
        this.inRange = inRange;
    }

    public void AddPrecondition(string key, object value)
    {
        preconditions.Add(key, value);
    }

    public void RemovePrecondition(string key)
    {
        if (preconditions.ContainsKey(key))
            preconditions.Remove(key);
    }

    public void AddEffect(string key, object value)
    {
        effects.Add (key, value);
    }

    public void RemoveEffect(string key)
    {
        if (effects.ContainsKey(key))
            effects.Remove(key);
    }
}
