using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GoalSeeker : MonoBehaviour
{
    Goal[] mGoals;
    SharkAction[] mActions;
    SharkAction mChangeOverTime;
    const float TICK_LENGTH = 5.0f;
    public TMP_Text stats;
    public TMP_Text action;
    public TMP_Text lowTitle;

    // Move to area
    public float moveSpeed = 5f;
    public Transform child;
    public Transform openWater;
    public Transform lifePreserver;
    
    public Transform speechBubble;
    public Camera mainCamera;
    void Start()
    {
        mGoals = new Goal[3];
        mGoals[0] = new Goal("Eat", 4);
        mGoals[1] = new Goal("Relax", 3);
        mGoals[2] = new Goal("Fun", 3);

        mActions = new SharkAction[3];
        mActions[0] = new SharkAction("attack this unattended child", child); //Move to area
        mActions[0].targetGoals.Add(new Goal("Eat", -2f));
        mActions[0].targetGoals.Add(new Goal("Relax", +0f));
        mActions[0].targetGoals.Add(new Goal("Fun", +1f));

        mActions[1] = new SharkAction("swim around", openWater);
        mActions[1].targetGoals.Add(new Goal("Eat", +2f));
        mActions[1].targetGoals.Add(new Goal("Relax", -4f));
        mActions[1].targetGoals.Add(new Goal("Fun", +2f));

        mActions[2] = new SharkAction("play with the life preserver", lifePreserver);
        mActions[2].targetGoals.Add(new Goal("Eat", 0f));
        mActions[2].targetGoals.Add(new Goal("Relax", 0f));
        mActions[2].targetGoals.Add(new Goal("Fun", -4f));

        mChangeOverTime = new SharkAction("tick");
        mChangeOverTime.targetGoals.Add(new Goal("Eat", +4f));
        mChangeOverTime.targetGoals.Add(new Goal("Relax", +1f));
        mChangeOverTime.targetGoals.Add(new Goal("Fun", +2f));

        Debug.Log("1 hour will pass every " + TICK_LENGTH + " seconds.");
        lowTitle.text = "1 hour will pass every " + TICK_LENGTH + " seconds.";
        InvokeRepeating("Tick", 0f, TICK_LENGTH);
        Debug.Log("Hit E to do something.");
    }

    void Tick()
    {
        foreach (Goal goal in mGoals)
        {
            goal.value += mChangeOverTime.GetGoalChange(goal);
            goal.value = Mathf.Max(goal.value, 0);
        }
        PrintGoals();
    }

    void PrintGoals()
    {
        string goalString = "";
        foreach (Goal goal in mGoals)
        {
            goalString += goal.name + ": " + goal.value + "; \n";
        }
        goalString += "Discontentment: " + CurrentDiscontentment();
        stats.text = goalString;
        Debug.Log(goalString);
    }



    void Update()
    {
        // Make the speech bubble always face the camera
        if (speechBubble != null && mainCamera != null)
        {
            Vector3 direction = mainCamera.transform.position - speechBubble.position;
            direction.y = 0;  // To keep it from tilting up/down, you can ignore the Y-axis rotation
            Quaternion rotation = Quaternion.LookRotation(direction);
            speechBubble.rotation = rotation;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SharkAction bestAction = ChooseAction(mActions, mGoals);
            Debug.Log("I'm going to " + bestAction.name);
            action.text = "I'm going to " + bestAction.name;

            // Update the speech bubble text
            speechBubble.GetComponentInChildren<TMP_Text>().text = "I'm going to " + bestAction.name;

            StartCoroutine(MoveToAction(bestAction));
        }
    }

    // Move to area
    IEnumerator MoveToAction(SharkAction bestAction)
    {
        Transform target = bestAction.targetPosition;
        
        while (Vector3.Distance(transform.position, target.position) > 0.1f)
        {
            // Move the shark towards the target
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            
            // Rotate the shark to face the target
            Vector3 direction = target.position - transform.position;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);  // Adjust rotation speed with 5f
            }

            yield return null;
        }

        // Apply the action's effects once the shark reaches the target
        foreach (Goal goal in mGoals)
        {
            goal.value += bestAction.GetGoalChange(goal);
            goal.value = Mathf.Max(goal.value, 0);
        }

        PrintGoals();
    }

    //Move to area
    SharkAction ChooseAction(SharkAction[] actions, Goal[] goals)
    {
        SharkAction bestAction = null;
        float bestValue = float.PositiveInfinity;

        foreach (SharkAction action in actions)
        {
            float thisValue = Discontentment(action, goals);
            if (thisValue < bestValue)
            {
                bestValue = thisValue;
                bestAction = action;
            }
        }
        return bestAction;
    }

    float Discontentment(SharkAction action, Goal[] goals)
    {
        float discontentment = 0f;

        foreach (Goal goal in goals)
        {
            float newValue = goal.value + action.GetGoalChange(goal);
            newValue = Mathf.Max(newValue, 0);
            discontentment += goal.GetDiscontentment(newValue);
        }
        return discontentment;
    }

    float CurrentDiscontentment()
    {
        float total = 0f;
        foreach (Goal goal in mGoals)
        {
            total += (goal.value * goal.value);
        }
        return total;
    }
}

//Moving to area
public class SharkAction
{
    public string name;
    public Transform targetPosition;
    public List<Goal> targetGoals = new List<Goal>();

    public SharkAction(string name, Transform targetPosition = null)
    {
        this.name = name;
        this.targetPosition = targetPosition;
    }

    public float GetGoalChange(Goal goal)
    {
        foreach (Goal g in targetGoals)
        {
            if (g.name == goal.name)
            {
                return g.value;
            }
        }
        return 0f;
    }
}
