using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class CaravaneerThreadPool : MonoBehaviour {
	public static CaravaneerThreadPool Instance;

	private static readonly object THREAD_LOCKER = new object ();

	private Queue<CaravaneerThread> functionsToBeRunInThread;
	private Queue<CaravaneerThread> functionsToBeResolved;

	private Thread newThread;
	private bool isRunning;

	void Awake(){
		Instance = this;
	}

	void Start () {
		this.isRunning = true;

		functionsToBeRunInThread = new Queue<CaravaneerThread> ();
		functionsToBeResolved = new Queue<CaravaneerThread> ();

		newThread = new Thread( RunThread );
		newThread.IsBackground = true;
		newThread.Start ();
	}
	
	void Update () {
		if(this.functionsToBeResolved.Count > 0){
			CaravaneerThread pathFind = this.functionsToBeResolved.Dequeue ();
			pathFind.Return ();
		}
	}

	public void AddToThreadPool(CaravaneerThread pathFindThread){
		lock (THREAD_LOCKER) {
			functionsToBeRunInThread.Enqueue (pathFindThread);
		}
	}

	private void RunThread(){
		while(isRunning){
			if(this.functionsToBeRunInThread.Count > 0){
				Thread.Sleep (2);
				CaravaneerThread newFunction = null;
				lock(THREAD_LOCKER){
					newFunction = this.functionsToBeRunInThread.Dequeue ();
				}
				if(newFunction != null){
					newFunction.ObtainCity();
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
