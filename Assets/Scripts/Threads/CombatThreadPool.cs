using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class CombatThreadPool : MonoBehaviour {
	public static CombatThreadPool Instance;

	private static readonly object THREAD_LOCKER = new object ();

	private Queue<ECS.CombatPrototype> functionsToBeRunInThread;
	private Queue<ECS.CombatPrototype> functionsToBeResolved;

	private Thread newThread;
	private bool isRunning;

	void Awake(){
		Instance = this;
	}

	void Start () {
		this.isRunning = true;

		functionsToBeRunInThread = new Queue<ECS.CombatPrototype> ();
		functionsToBeResolved = new Queue<ECS.CombatPrototype> ();

		newThread = new Thread( RunThread );
		newThread.IsBackground = true;
		newThread.Start ();
	}
	
	void Update () {
		if(this.functionsToBeResolved.Count > 0){
			ECS.CombatPrototype combat = this.functionsToBeResolved.Dequeue ();
			combat.ReturnCombatResults ();
		}
	}

	public void AddToThreadPool(ECS.CombatPrototype combat){
		lock (THREAD_LOCKER) {
			functionsToBeRunInThread.Enqueue (combat);
		}
	}

	private void RunThread(){
		while(isRunning){
			if(this.functionsToBeRunInThread.Count > 0){
				Thread.Sleep (2);
				ECS.CombatPrototype newFunction = null;
				lock(THREAD_LOCKER){
					newFunction = this.functionsToBeRunInThread.Dequeue ();
				}
				if(newFunction != null){
					newFunction.CombatSimulation();
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
