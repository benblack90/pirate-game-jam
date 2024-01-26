using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public struct Task
{
    public string taskName;
    public bool isCompleted;
}

public class TaskList : MonoBehaviour
{
    List<Task> currentTasks = new List<Task>();
    [SerializeField] List<Task> preGooTasks = new List<Task>();
    [SerializeField] List<Task> postGooTasks = new List<Task>();
    [SerializeField] TextMeshProUGUI taskText;

    private void Start()
    {
        currentTasks = preGooTasks;
        UpdateTaskText();
    }
    private void OnEnable()
    {
        Collectable.onGenericCollectable += KeyCardTask;
        TriggerArea.onPlayerEnterTrigger += ChamberOpenTask;
        GooChamber.onGooRelease += GooReleaseTask;

    }

    private void OnDisable()
    {
        Collectable.onGenericCollectable -= KeyCardTask;
        TriggerArea.onPlayerEnterTrigger -= ChamberOpenTask;
        GooChamber.onGooRelease -= GooReleaseTask;

    }
    void UpdateTaskText()
    {
        string totalTasks = "";
        foreach (var task in currentTasks)
        {
            if (task.isCompleted)
            {
                totalTasks = totalTasks + "-<s>" + task.taskName + "</s> \n";
            }
            else
            {
                totalTasks = totalTasks  +"-"+ task.taskName + "\n";

            }
        }

        taskText.text = totalTasks;
    }

    void CompleteTask(int taskID)
    {
        Task cardTask = currentTasks[taskID];
        cardTask.isCompleted = true;
        currentTasks[taskID] = cardTask;

        UpdateTaskText();
    }

    void KeyCardTask(string obj)
    {
        if(obj == "GooCard")
        {
            CompleteTask(0);
        }
    }

    void ChamberOpenTask(string trig)
    {
        if (trig == "Chamber")
        {
            CompleteTask(1);
        }
    }

    void GooReleaseTask()
    {
        CompleteTask(2);
        StartCoroutine(TaskSwap());
    }

    IEnumerator TaskSwap()
    {
        WaitForSeconds wait = new WaitForSeconds(1);
        yield return wait;
        currentTasks = postGooTasks;
        UpdateTaskText();
    }


}
