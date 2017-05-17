using UnityEngine;
using System.Collections;

public class StartupManager : MonoBehaviour {
	public MapGenerator mapGenerator;

	IEnumerator Start(){
		while(!LocalizationManager.Instance.GetIsReady()){
			yield return null;
		}

		//this.mapGenerator.InitializeWorld ();
	}
}
