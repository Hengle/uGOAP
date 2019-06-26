using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GoapAgent : MonoBehaviour
{
    private FSM stateMachine;
    private FSM.FSMState idleState;  // finds something to do
    private FSM.FSMState moveToState;  // moves to a target
    private FSM.FSMState performActionState;  // performs an action
    
    private HashSet<GoapAction> availableActions;
    private Queue<GoapAction> currentActions;

    private IGoap dataProvider;  // implementing class that provides our world data and listens to feedback on planning

    private GoapPlanner planner;


    void Start()
    {
        stateMachine = new FSM();
        availableActions = new HashSet<GoapAction>();
        currentActions = new Queue<GoapAction>();
        planner = new GoapPlanner();
        FindDataProvider();
        CreateIdleState();
        CreateMoveToState();
        CreatePerformActionState();
        stateMachine.PushState(idleState);
        LoadActions();
    }
    

    void Update()
    {
        stateMachine.Update(gameObject);
    }


    public void AddAction(GoapAction a)
    {
        availableActions.Add(a);
    }

    public GoapAction GetAction(Type action)
    {
        foreach (GoapAction g in availableActions)
        {
            if (g.GetType().Equals(action))
                return g;
        }
        return null;
    }

    public void RemoveAction(GoapAction action)
    {
        availableActions.Remove(action);
    }

    private bool HasActionPlan()
    {
        return currentActions.Count > 0;
    }

    private void CreateIdleState()
    {
        idleState = (fsm, gameObj) => 
        {
            // GOAP planning

            // get the world state and the goal we want to plan for
            Dictionary<string, object> worldState = dataProvider.GetWorldState();
            KeyValuePair<string, object> goal = dataProvider.GetCurrentGoal();

            // search enable Plan
            Queue<GoapAction> plan = planner.Plan(gameObject, availableActions, worldState, goal, dataProvider);

            if (plan != null)
            {
                // we have a plan, hooray!
                currentActions = plan;
                dataProvider.PlanFound(goal, plan);

                fsm.PopState();  // move to PerformAction state
                fsm.PushState(performActionState);
            }
            else
            {
                // ugh, we couldn't get a plan
                Debug.Log("[" + this.name + "] " + "<color=orange>Plan failed:</color> " + PrettyPrint(goal) + " = NO PLAN");
                dataProvider.PlanFailed(goal);
                fsm.PopState();  // move back to IdleAction state
                fsm.PushState(idleState);
            }

        };
    }
    
    private void CreateMoveToState()
    {
        // move the game object
        moveToState = (fsm, gameObj) => 
        {
            GoapAction action = currentActions.Peek();

            // get the agent to move itself
            if (dataProvider.MoveAgent(action))
            {
                fsm.PopState();
            }
        };
    }
    
    private void CreatePerformActionState()
    {
        performActionState = (fsm, gameObj) => 
        {
            // perform the action
            if (!HasActionPlan())
            {
                // no actions to perform
                Debug.Log("<color=red>Done actions</color>");
                fsm.PopState();
                fsm.PushState(idleState);
                dataProvider.ActionsFinished();
                return;
            }

            GoapAction action = currentActions.Peek();
            if (action.IsDone())
            {
                // the action is done. Remove it so we can perform the next one
                currentActions.Dequeue();
            }

            if (HasActionPlan())
            {
                // perform the next action
                action = currentActions.Peek();
                bool inRange = action.RequiresInRange() ? action.IsInRange() : true;

                if (inRange)
                {
                    // we are in range, so perform the action
                    bool success = action.Perform(gameObj, dataProvider.GetMemory());

                    if (!success)
                    {
                        // action failed, we need to plan again
                        fsm.PopState();
                        fsm.PushState(idleState);
                        dataProvider.PlanAborted(action);
                    }
                }
                else
                {
                    // we need to move there first
                    // push moveTo state
                    fsm.PushState(moveToState);
                }
            }
            else
            {
                // no actions left, move to Plan state
                fsm.PopState();
                fsm.PushState(idleState);
                dataProvider.ActionsFinished();
            }
        };
    }

    private void FindDataProvider()
    {
        foreach (Component comp in gameObject.GetComponents(typeof(Component)))
        {
            if ( typeof(IGoap).IsAssignableFrom(comp.GetType()) )
            {
                dataProvider = (IGoap)comp;
                return;
            }
        }
    }

    private void LoadActions()
    {
        GoapAction[] actions = gameObject.GetComponents<GoapAction>();
        foreach (GoapAction a in actions)
            availableActions.Add(a);
        Debug.Log("[" + this.name + "] " + "Found actions: " + PrettyPrint(actions));
    }

    public void AbortFsm()
    {
        stateMachine.ClearState();
        stateMachine.PushState(idleState);
    }

    public static string PrettyPrint(HashSet<KeyValuePair<string, object>> state)
    {
        string s = "";
        foreach (KeyValuePair<string, object> kvp in state)
        {
            s += kvp.Key + ":" + kvp.Value;
            s += ", ";
        }
        return s;
    }

    public static string PrettyPrint(Dictionary<string, object> goals)
    {
        string s = "";
        foreach (var g in goals)
        {
            s += g.Key + g.Value.ToString();
            s += ", ";
        }
        return s;
    }

    public static string PrettyPrint(KeyValuePair<string, object> goal)
    {
        return goal.Key + ":" + goal.Value.ToString();
    }

    public static string PrettyPrint(Queue<GoapAction> actions)
    {
        string s = "";
        foreach (GoapAction a in actions)
        {
            s += a.GetType().Name;
            s += "->";
        }
        s = s.Remove(s.Length - 2);  // removes '->' after last action
        return s;
    }

    public static string PrettyPrint(GoapAction[] actions)
    {
        string s = "";
        foreach (GoapAction a in actions)
        {
            s += a.GetType().Name;
            s += ", ";
        }
        return s;
    }

    public static string PrettyPrint(GoapAction action)
    {
        string s = "" + action.GetType().Name;
        return s;
    }
}
