using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Panda;

public class BehaviourTreeManager : MonoBehaviour {
	public static BehaviourTreeManager Instance;
	public List<PandaBehaviour> allTrees =  new List<PandaBehaviour>();

	void Awake(){
		Instance = this;
	}

	public void Tick(){
		for(int i = 0; i < this.allTrees.Count; i++){
			this.allTrees [i].Tick ();
		}
	}
}
