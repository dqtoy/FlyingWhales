using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class MultiThreadPool : MonoBehaviour {
    public static MultiThreadPool Instance;

    private static readonly object THREAD_LOCKER = new object();

    private Queue<Multithread> functionsToBeRunInThread;
    private Queue<Multithread> functionsToBeResolved;

    private Thread newThread;
    private bool isRunning;

    void Awake() {
        Instance = this;
        this.isRunning = true;

        functionsToBeRunInThread = new Queue<Multithread>();
        functionsToBeResolved = new Queue<Multithread>();

        newThread = new Thread(RunThread);
        newThread.IsBackground = true;
        newThread.Start();
    }

    void LateUpdate() {
        if (this.functionsToBeResolved.Count > 0) {
            Multithread action = this.functionsToBeResolved.Dequeue();
            action.FinishMultithread();
        }
    }

    public void AddToThreadPool(Multithread multiThread) {
        lock (THREAD_LOCKER) {
            functionsToBeRunInThread.Enqueue(multiThread);
        }
    }

    private void RunThread() {
        while (isRunning) {
            if (this.functionsToBeRunInThread.Count > 0) {
                Thread.Sleep(5);
                Multithread newFunction = null;
                lock (THREAD_LOCKER) {
                    newFunction = this.functionsToBeRunInThread.Dequeue();
                }
                if (newFunction != null) {
                    newFunction.DoMultithread();
                }
                lock (THREAD_LOCKER) {
                    this.functionsToBeResolved.Enqueue(newFunction);
                }

            }
        }
    }

    void OnDestroy() {
        this.isRunning = false;
    }
}
