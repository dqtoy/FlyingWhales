using UnityEngine;
using System.Collections;

public enum PILLAGE_ACTION { OBTAIN_ITEM, END, CIVILIAN_DIES, NOTHING }

public class TaskManager : MonoBehaviour {
	public static TaskManager Instance;

	private WeightedDictionary<PILLAGE_ACTION> _pillageActions;

	#region getters/setters
	public WeightedDictionary<PILLAGE_ACTION> pillageActions{
		get { return _pillageActions; }
	}
	#endregion
	void Awake(){
		Instance = this;
	}
		
	internal void Initialize(){
		ConstructData ();
	}

	private void ConstructData(){
		_pillageActions = new WeightedDictionary<PILLAGE_ACTION>();
		_pillageActions.AddElement(PILLAGE_ACTION.OBTAIN_ITEM, 20);
		_pillageActions.AddElement(PILLAGE_ACTION.END, 15);
		_pillageActions.AddElement(PILLAGE_ACTION.CIVILIAN_DIES, 15);
		_pillageActions.AddElement(PILLAGE_ACTION.NOTHING, 70);
	}
}
