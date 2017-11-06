using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourcesManager : MonoBehaviour {
	public static ResourcesManager Instance;

	[SerializeField] private ResourceAndRegionCap[] foodCap;
	[SerializeField] private ResourceAndRegionCap[] materialsCap;
	[SerializeField] private ResourceAndRegionCap[] oresCap;

	public Dictionary<RESOURCE, int> resourceCapDict = new Dictionary<RESOURCE, int>();
	public Dictionary<RESOURCE, int> resourceCountDict = new Dictionary<RESOURCE, int>();
	void Awake(){
		Instance = this;
	}
	void Start(){
		for (int i = 0; i < foodCap.Length; i++) {
			resourceCapDict.Add (foodCap [i].resource, foodCap [i].capacity);
			resourceCountDict.Add (foodCap [i].resource, 0);
		}
		for (int i = 0; i < materialsCap.Length; i++) {
			resourceCapDict.Add (materialsCap [i].resource, materialsCap [i].capacity);
			resourceCountDict.Add (materialsCap [i].resource, 0);
		}
		for (int i = 0; i < oresCap.Length; i++) {
			resourceCapDict.Add (oresCap [i].resource, oresCap [i].capacity);
			resourceCountDict.Add (oresCap [i].resource, 0);
		}
	}


	/// <summary>
	/// This will add resource count to track how many regions will only have a certain resource,
	/// return TRUE if resource count is added,
	/// return FALSE if resource count reached limit
	/// </summary>
	internal bool AddResourceCount(RESOURCE resource){
		if(this.resourceCountDict[resource] < this.resourceCapDict[resource]){
			this.resourceCountDict [resource]++;
			return true;
		}else{
			return false;
		}
	}
}
