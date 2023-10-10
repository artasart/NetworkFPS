using System.Collections.Generic;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static MainThreadDispatcher instance;

    private Queue<System.Action> actionQueue = new Queue<System.Action>();

    private object queueLock = new object();

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        lock (queueLock)
        {
            while (actionQueue.Count > 0)
            {
                var action = actionQueue.Dequeue();

                action?.Invoke();
            }
        }
    }

    public static void RunOnMainThread(System.Action action)
    {
        lock (instance.queueLock)
        {
            instance.actionQueue.Enqueue(action);
        }
    }
}