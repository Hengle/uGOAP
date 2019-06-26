using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/**
 * Collect the world data for this Agent that will be
 * used for GOAP planning.
 */


/**
 * Any agent that wants to use GOAP must implement
 * this interface. It provides information to the GOAP
 * planner so it can plan what actions to use.
 * 
 * It also provides an interface for the planner to give 
 * feedback to the Agent and report success/failure.
 */
public interface IGoap
{
    /**
     * The starting state of the Agent and the world.
     * Supply what states are needed for actions to run.
     */
    Dictionary<string, object> GetWorldState();

    /**
     * Create a new goal for agent
     */
    void AddGoal(string goalName, object value);

    /**
     * Remove goal from agent
     */
    bool RemoveGoal(string goalName);

    /**
     * Set current goal for agent
     */
    bool SetCurrentGoal(string goalName, object value);

    /**
     * Get agent`s current goal.
     * Give the planner a new goal so it can figure out the actions needed to fulfill it.
     */
    KeyValuePair<string, object> GetCurrentGoal();

    /**
     * Get blackboard for environment
     */
    GoapMemory GetMemory();

    /**
     * No sequence of actions could be found for the supplied goal.
     * You will need to try another goal
     */
    void PlanFailed(KeyValuePair<string, object> failedGoal);

    /**
     * A plan was found for the supplied goal.
     * These are the actions the Agent will perform, in order.
     */
    void PlanFound(KeyValuePair<string, object> goal, Queue<GoapAction> actions);

    /**
     * All actions are complete and the goal was reached. Hooray!
     */
    void ActionsFinished();

    /**
     * One of the actions caused the plan to abort.
     * That action is returned.
     */
    void PlanAborted(GoapAction aborter);

    /**
     * Called during Update. Move the agent towards the target in order
     * for the next action to be able to perform.
     * Return true if the Agent is at the target and the next action can perform.
     * False if it is not there yet.
     */
    bool MoveAgent(GoapAction nextAction);

    void Init();
}
