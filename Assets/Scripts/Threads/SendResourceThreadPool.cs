using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class SendResourceThreadPool : MonoBehaviour {
	public static SendResourceThreadPool Instance;

	private static readonly object THREAD_LOCKER = new object ();

	private Queue<SendResourceThread> functionsToBeRunInThread;
	private Queue<SendResourceThread> functionsToBeResolved;

	private Thread newThread;
	private bool isRunning;

	void Awake(){
		Instance = this;
	}

	void Start () {
		this.isRunning = true;

		functionsToBeRunInThread = new Queue<SendResourceThread> ();
		functionsToBeResolved = new Queue<SendResourceThread> ();

		newThread = new Thread( RunThread );
		newThread.IsBackground = true;
		newThread.Start ();
	}
	
	void Update () {
		if(this.functionsToBeResolved.Count > 0){
			SendResourceThread pathFind = this.functionsToBeResolved.Dequeue ();
			pathFind.ReturnResource ();
		}
	}

	public void AddToThreadPool(SendResourceThread pathFindThread){
		lock (THREAD_LOCKER) {
			functionsToBeRunInThread.Enqueue (pathFindThread);
		}
	}

	private void RunThread(){
		while(isRunning){
			if(this.functionsToBeRunInThread.Count > 0){
				Thread.Sleep (2);
				SendResourceThread newFunction = null;
				lock(THREAD_LOCKER){
					newFunction = this.functionsToBeRunInThread.Dequeue ();
				}
				if(newFunction != null){
					newFunction.SendResource();
				}
				lock (THREAD_LOCKER) {
					this.functionsToBeResolved.Enqueue (newFunction);
				}

			}
		}
	}

	void OnDestroy(){
		this.isRunning = false;
	}
}
