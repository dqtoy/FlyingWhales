using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourcesManager : MonoBehaviour {
	public static ResourcesManager Instance;

	[SerializeField] private ResourceAndRegionCap[] foodCap;
	[SerializeField] private ResourceAndRegionCap[] materialsCap;
	[SerializeField] private ResourceAndRegionCap[] oresCap;

	public Dictionary<RESOURCE, int> resourceCapDict = new Dictionary<RESOURCE, int>();
	void Awake(){
		Instance = this;
		for (int i = 0; i < foodCap.Length; i++) {
			resourceCapDict.Add (foodCap [i].resource, foodCap [i].capacity);
		}
		for (int i = 0; i < materialsCap.Length; i++) {
			resourceCapDict.Add (materialsCap [i].resource, materialsCap [i].capacity);
		}
		for (int i = 0; i < oresCap.Length; i++) {
			resourceCapDict.Add (oresCap [i].resource, oresCap [i].capacity);
		}
	}

	/// <summary>
	/// This will reduce resource count to track how many regions will only have a certain resource,
	/// return TRUE if resource count is reduced,
	/// return FALSE if resource count reached limit
	/// </summary>
	internal bool ReduceResourceCount(RESOURCE resource){
		if(this.resourceCapDict[resource] > 0){
			this.resourceCapDict [resource]--;
			return true;
		}else{
			return false;
		}
	}
}
