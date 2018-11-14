using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;

public class CombatThreadPool : MonoBehaviour {
	public static CombatThreadPool Instance;

	private static readonly object THREAD_LOCKER = new object ();

	private Queue<ECS.Combat> functionsToBeRunInThread;
	private Queue<ECS.Combat> functionsToBeResolved;

	private Thread newThread;
	private bool isRunning;

	void Awake(){
		Instance = this;
	}

	void Start () {
		this.isRunning = true;

		functionsToBeRunInThread = new Queue<ECS.Combat> ();
		functionsToBeResolved = new Queue<ECS.Combat> ();

		newThread = new Thread( RunThread );
		newThread.IsBackground = true;
		newThread.Start ();
	}
	
	//void Update () {
	//	if(this.functionsToBeResolved.Count > 0){
	//		ECS.Combat combat = this.functionsToBeResolved.Dequeue ();
	//		combat.ReturnCombatResults ();
	//	}
	//}

	public void AddToThreadPool(ECS.Combat combat){
		lock (THREAD_LOCKER) {
			functionsToBeRunInThread.Enqueue (combat);
		}
	}

	private void RunThread(){
		//while(isRunning){
		//	if(this.functionsToBeRunInThread.Count > 0){
		//		Thread.Sleep (2);
		//		ECS.Combat newFunction = null;
		//		lock(THREAD_LOCKER){
		//			newFunction = this.functionsToBeRunInThread.Dequeue ();
		//		}
		//		//if(newFunction != null){
		//		//	newFunction.CombatSimulation();
		//		//}
		//		lock (THREAD_LOCKER) {
		//			this.functionsToBeResolved.Enqueue (newFunction);
		//		}

		//	}
		//}
	}

	void OnDestroy(){
		this.isRunning = false;
	}
}
