using UnityEngine;
using System.Collections;

public class Spy : Role {

	public int successfulMissions;
	public int unsuccessfulMissions;
	public bool inAction;

	private int actionDurationInWeeks;

	protected delegate void DoAction();
	protected DoAction onDoAction;

	private RelationshipKingdom warExhaustiontarget;

	public Spy(Citizen citizen): base(citizen){
		this.successfulMissions = 0;
		this.unsuccessfulMissions = 0;
		this.inAction = false;
		this.actionDurationInWeeks = 0;
	}

	internal void StartDecreaseWarExhaustionTask(RelationshipKingdom targetKingdom){
		Debug.Log(this.citizen.name + ": Start Decrease War Exhaustion Task");
		this.actionDurationInWeeks = 2;
		this.warExhaustiontarget = targetKingdom;
		this.inAction = true;
		this.onDoAction += DecreaseWarExhaustion;
		EventManager.Instance.onWeekEnd.AddListener(WaitForAction);
	}

	protected void DecreaseWarExhaustion(){
//		if (this.citizen.skillTraits.Contains (SKILL_TRAIT.STEALTHY)) {
//			this.warExhaustiontarget.kingdomWar.exhaustion -= 20;
//			Debug.Log (this.citizen.name + ": Decreased War Exhaustion of " + this.warExhaustiontarget.sourceKingdom.name + " by 20");
//		} else {
			this.warExhaustiontarget.kingdomWar.exhaustion -= 15;
			Debug.Log (this.citizen.name + ": Decreased War Exhaustion of " + this.warExhaustiontarget.sourceKingdom.name + " by 15");
//		}
		this.inAction = false;
		this.onDoAction -= DecreaseWarExhaustion;
		EventManager.Instance.onWeekEnd.RemoveListener(WaitForAction);
	}

	protected void WaitForAction(){
		if (this.actionDurationInWeeks > 0) {
			this.actionDurationInWeeks -= 1;
		} else {
			this.onDoAction();
		}
	}
}
