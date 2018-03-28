using UnityEngine;
using System.Collections;

public enum PILLAGE_ACTION { OBTAIN_ITEM, END, CIVILIAN_DIES, NOTHING }
public enum HUNT_ACTION { EAT, END, NOTHING }

public class TaskManager : MonoBehaviour {
	public static TaskManager Instance;

	private WeightedDictionary<PILLAGE_ACTION> _pillageActions;
	private WeightedDictionary<HUNT_ACTION> _huntActions;
	private WeightedDictionary<string> _vampiricEmbraceActions;
	private WeightedDictionary<string> _drinkBloodActions;
	private WeightedDictionary<string> _hypnotizeActions;
	private WeightedDictionary<string> _stealActions;
	private WeightedDictionary<string> _stealAttemptActions;


	#region getters/setters
	public WeightedDictionary<PILLAGE_ACTION> pillageActions{
		get { return _pillageActions; }
	}
	public WeightedDictionary<HUNT_ACTION> huntActions{
		get { return _huntActions; }
	}
	public WeightedDictionary<string> vampiricEmbraceActions{
		get { return _vampiricEmbraceActions; }
	}
	public WeightedDictionary<string> drinkBloodActions{
		get { return _drinkBloodActions; }
	}
	public WeightedDictionary<string> hypnotizeActions{
		get { return _hypnotizeActions; }
	}
	public WeightedDictionary<string> stealActions{
		get { return _stealActions; }
	}
	public WeightedDictionary<string> stealAttemptActions{
		get { return _stealAttemptActions; }
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

		_huntActions = new WeightedDictionary<HUNT_ACTION>();
		_huntActions.AddElement(HUNT_ACTION.EAT, 15);
		_huntActions.AddElement(HUNT_ACTION.END, 15);
		_huntActions.AddElement(HUNT_ACTION.NOTHING, 70);

		_vampiricEmbraceActions = new WeightedDictionary<string>();
		_vampiricEmbraceActions.AddElement ("turn", 15);
		_vampiricEmbraceActions.AddElement ("caught", 15);
		_vampiricEmbraceActions.AddElement("nothing", 50);

		_drinkBloodActions = new WeightedDictionary<string>();
		_drinkBloodActions.AddElement ("drink", 15);
		_drinkBloodActions.AddElement ("caught", 10);
		_drinkBloodActions.AddElement("nothing", 50);

		_hypnotizeActions = new WeightedDictionary<string>();
		_hypnotizeActions.AddElement ("hypnotize", 15);
		_hypnotizeActions.AddElement("nothing", 50);

		_stealActions = new WeightedDictionary<string>();
		_stealActions.AddElement ("steal", 10);
		_stealActions.AddElement("nothing", 30);

		_stealAttemptActions = new WeightedDictionary<string>();
		_stealAttemptActions.AddElement ("success not caught", 20);
		_stealAttemptActions.AddElement ("success caught", 5);
		_stealAttemptActions.AddElement ("failed not caught", 10);
		_stealAttemptActions.AddElement ("failed caught", 5);
	}
}
