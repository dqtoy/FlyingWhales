using UnityEngine;
using System.Collections;

public class PlayerInterventionManager : MonoBehaviour {
	public static PlayerInterventionManager Instance;

	void Awake () {
		Instance = this;
	}
	

}
