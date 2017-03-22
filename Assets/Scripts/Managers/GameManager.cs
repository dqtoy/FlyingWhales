using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public static GameManager Instance = null;

	void Awake(){
		Instance = this;
	}


}
