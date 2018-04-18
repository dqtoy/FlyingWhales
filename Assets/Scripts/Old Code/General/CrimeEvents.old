using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrimeEvents : MonoBehaviour {
	public static CrimeEvents Instance;
	public List<CrimeData> crimes;

	void Awake(){
		Instance = this;
	}

	public CrimeData GetRandomCrime(){
		return this.crimes [UnityEngine.Random.Range (0, this.crimes.Count)];
	}
}
