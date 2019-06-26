using System.Collections.Generic;
using UnityEngine;

/**
 * Plans what actions can be completed in order to fulfill a goal state.
 */
public class GoapPlanner
{
    /**
     * Plan what sequence of actions can fulfill the goal.
     * Returns null if a plan could not be found, or a list of the actions
     * that must be performed, in order, to fulfill the goal.
     */
    public Queue<GoapAction> Plan(GameObject agent, 
                                  HashSet<GoapAction> availableActions, 
                                  Dictionary<string, object> worldState, 
                                  KeyValuePair<string, object> goal,
                                  IGoap goap) 
    {
        // reset the actions so we can start fresh with them
        foreach (GoapAction a in availableActions)
            a.DoReset();

        // check what actions can run using their checkProceduralPrecondition
        HashSet<GoapAction> usableActions = NodeManager.GetFreeActionSet();
        foreach (GoapAction a in availableActions)
        {
            if ( a.CheckProceduralPrecondition(agent, goap.GetMemory()) )
                usableActions.Add(a);
        }
        
        // we now have all actions that can run, stored in usableActions

        // build up the tree and record the leaf nodes that provide a solution to the goal.
        List<GoapNode> leaves = new List<GoapNode>();

        // build graph
        GoapNode start = NodeManager.GetFreeNode(null, 0, worldState, null);
        bool success = BuildGraph(start, leaves, usableActions, goal);

        if (!success)
        {
            // oh no, we didn't get a plan
            Debug.Log("[" + agent.name + "] " + "NO PLAN");
            return null;
        }

        // get the cheapest leaf
        GoapNode cheapest = null;
        foreach (GoapNode leaf in leaves)
        {
            if (cheapest == null)
                cheapest = leaf;
            else
            {
                if (leaf.BetterThen(cheapest))
                    cheapest = leaf;
            }
        }

        // get its node and work back through the parents
        List<GoapAction> result = new List<GoapAction>();
        GoapNode n = cheapest;
        while (n != null)
        {
            if (n.action != null)
            {
                result.Insert(0, n.action); // insert the action in the front
            }
            n = n.parent;
        }
        NodeManager.Release();
        // we now have this action list in correct order

        Queue<GoapAction> queue = new Queue<GoapAction>();
        foreach (GoapAction a in result)
            queue.Enqueue(a);
        
        // Builds a shortest way for many actions
        //for (int i = 1; i < result.Count; i++)
        //{
        //    GoapAction a = result[i];
        //    a.CheckDistance(result[i-1].target, goap.GetMemory());
        //}

        // hooray we have a plan!
        return queue;
    }

    /**
     * Returns true if at least one solution was found.
     * The possible paths are stored in the leaves list. Each leaf has a
     * 'runningCost' value where the lowest Cost will be the best action
     * sequence.
     */
    private bool BuildGraph(GoapNode parent, List<GoapNode> leaves, HashSet<GoapAction> usableActions, KeyValuePair<string, object> goal)
    {
        bool foundOne = false;

        // go through each action available at this node and see if we can use it here
        foreach (GoapAction action in usableActions)
        {
            // if the parent state has the conditions for this action's preconditions, we can use it here
            if ( InState(action.Preconditions, parent.state) )
            {

                // apply the action's effects to the parent state
                Dictionary<string, object> currentState = PopulateState(parent.state, action.Effects);
                //Debug.Log(GoapAgent.prettyPrint(currentState));
                GoapNode node = NodeManager.GetFreeNode(parent, parent.runningCost + action.GetCost(), currentState, action);

                //force child.precondition in parent.effects or child.precondition is empty.
                if (action.Preconditions.Count == 0 && parent.action != null ||
                    parent.action != null && !CondRelation(action.Preconditions, parent.action.Effects))
                    continue;

                if (FillGoal(goal, currentState))
                {
                    // we found a solution!
                    leaves.Add(node);
                    foundOne = true;
                }
                else
                {
                    // not at a solution yet, so test all the remaining actions and branch out the tree
                    HashSet<GoapAction> subset = ActionSubset(usableActions, action);
                    bool found = BuildGraph(node, leaves, subset, goal);
                    if (found)
                        foundOne = true;
                }
            }
        }

        return foundOne;
    }

    // if there is one true relationship
    private bool CondRelation(Dictionary<string, object> preconditions, Dictionary<string, object> effects)
    {
        foreach (var t in preconditions)
        {
            var match = effects.ContainsKey(t.Key) && effects[t.Key].Equals(t.Value);
            if (match)
                return true;
        }
        return false;
    }

    /**
     * Create a subset of the actions excluding the removeMe one. Creates a new set.
     */
    private HashSet<GoapAction> ActionSubset(HashSet<GoapAction> actions, GoapAction removeMe)
    {
        HashSet<GoapAction> subset = NodeManager.GetFreeActionSet();
        foreach (GoapAction a in actions)
        {
            if (!a.Equals(removeMe))
                subset.Add(a);
        }
        return subset;
    }

    /**
     * Check that all items in 'test' are in 'state'. If just one does not match or is not there
     * then this returns false.
     */
    private bool InState(Dictionary<string, object> test, Dictionary<string, object> state)
    {
        bool allMatch = true;
        foreach (var t in test)
        {
            bool match = state.ContainsKey(t.Key) && state[t.Key].Equals(t.Value);
            if (!match)
            {
                allMatch = false;
                break;
            }
        }
        return allMatch;
    }

    private bool FillGoal(KeyValuePair<string, object> goal, Dictionary<string, object> state)
    {
        var match = state.ContainsKey(goal.Key) && state[goal.Key].Equals(goal.Value);
        return match;
    }
    
    /**
     * Apply the stateChange to the currentState
     */
    private Dictionary<string, object> PopulateState(Dictionary<string, object> currentState, 
                                                               Dictionary<string, object> stateChange)
    {
        Dictionary<string, object> state = NodeManager.GetFreeState();
        state.Clear();
        // copy the KVPs over as new objects
        foreach (KeyValuePair<string,object> s in currentState)
            state.Add(s.Key, s.Value);

        foreach (KeyValuePair<string, object> change in stateChange)
        {
            // if the key exists in the current state, update the Value
            if (state.ContainsKey(change.Key))
            {
                state[change.Key] = change.Value;
            }
            else
            {
                state.Add(change.Key,change.Value);
            }
        }
        return state;
    }

}
