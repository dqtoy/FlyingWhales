using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class PathfindingThreadPool : MonoBehaviour {
	public static PathfindingThreadPool Instance;

	private static readonly object THREAD_LOCKER = new object ();

	private Queue<PathFindingThread> functionsToBeRunInThread;
	private Queue<PathFindingThread> functionsToBeResolved;

	private Thread newThread;
	private bool isRunning;

	void Awake(){
		Instance = this;
	}

	void Start () {
		this.isRunning = true;

		functionsToBeRunInThread = new Queue<PathFindingThread> ();
		functionsToBeResolved = new Queue<PathFindingThread> ();

		newThread = new Thread( RunThread );
		newThread.IsBackground = true;
		newThread.Start ();
	}
	
	void Update () {
		if(this.functionsToBeResolved.Count > 0){
			PathFindingThread pathFind = this.functionsToBeResolved.Dequeue ();
			pathFind.ReturnPath ();
		}
	}

	public void AddToThreadPool(PathFindingThread pathFindThread){
		lock (THREAD_LOCKER) {
			functionsToBeRunInThread.Enqueue (pathFindThread);
		}
	}

	private void RunThread(){
		while(isRunning){
			if(this.functionsToBeRunInThread.Count > 0){
				Thread.Sleep (2);
				PathFindingThread newFunction = null;
				lock(THREAD_LOCKER){
					newFunction = this.functionsToBeRunInThread.Dequeue ();
				}
				if(newFunction != null){
					newFunction.FindPath();
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
