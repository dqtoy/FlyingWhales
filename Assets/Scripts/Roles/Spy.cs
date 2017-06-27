using UnityEngine;
using System.Collections;

public class Spy : Role {
	public Assassination assassination;
//	public int successfulMissions;
//	public int unsuccessfulMissions;
//	public bool inAction;
//
//	private int actionDurationInWeeks;
//
//	protected delegate void DoAction();
//	protected DoAction onDoAction;
//
//	private RelationshipKingdom warExhaustiontarget;

	public Spy(Citizen citizen): base(citizen){
		this.assassination = null;
//		this.successfulMissions = 0;
//		this.unsuccessfulMissions = 0;
//		this.inAction = false;
//		this.actionDurationInWeeks = 0;
	}
	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Assassination){
            base.Initialize(gameEvent);
			this.assassination = (Assassination)gameEvent;
			this.assassination.spy = this;
			this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Spy"), this.citizen.city.hexTile.transform) as GameObject;
			this.avatar.transform.localPosition = Vector3.zero;
			this.avatar.GetComponent<SpyAvatar>().Init(this);
		}
	}

	internal override void Attack (){
//		base.Attack ();
		if(this.avatar != null){
			this.avatar.GetComponent<SpyAvatar> ().HasAttacked();
			if(this.avatar.GetComponent<SpyAvatar> ().direction == DIRECTION.LEFT){
				this.avatar.GetComponent<SpyAvatar> ().animator.Play ("Attack_Left");
			}else if(this.avatar.GetComponent<SpyAvatar> ().direction == DIRECTION.RIGHT){
				this.avatar.GetComponent<SpyAvatar> ().animator.Play ("Attack_Right");
			}else if(this.avatar.GetComponent<SpyAvatar> ().direction == DIRECTION.UP){
				this.avatar.GetComponent<SpyAvatar> ().animator.Play ("Attack_Up");
			}else{
				this.avatar.GetComponent<SpyAvatar> ().animator.Play ("Attack_Down");
			}
		}
	}
	/*internal void StartDecreaseWarExhaustionTask(RelationshipKingdom targetKingdom){
//		Debug.Log(this.citizen.name + ": Start Decrease War Exhaustion Task");
		this.actionDurationInWeeks = 2;
		this.warExhaustiontarget = targetKingdom;
		this.inAction = true;
		this.onDoAction += DecreaseWarExhaustion;
		EventManager.Instance.onWeekEnd.AddListener(WaitForAction);
	}*/

	/*protected void DecreaseWarExhaustion(){
//		if (this.citizen.skillTraits.Contains (SKILL_TRAIT.STEALTHY)) {
//			this.warExhaustiontarget.kingdomWar.exhaustion -= 20;
//			Debug.Log (this.citizen.name + ": Decreased War Exhaustion of " + this.warExhaustiontarget.sourceKingdom.name + " by 20");
//		} else {
			this.warExhaustiontarget.kingdomWar.exhaustion -= 15;
//			Debug.Log (this.citizen.name + ": Decreased War Exhaustion of " + this.warExhaustiontarget.sourceKingdom.name + " by 15");
//		}
		this.inAction = false;
		this.onDoAction -= DecreaseWarExhaustion;
		EventManager.Instance.onWeekEnd.RemoveListener(WaitForAction);
	}*/

	/*protected void WaitForAction(){
		if (this.actionDurationInWeeks > 0) {
			this.actionDurationInWeeks -= 1;
		} else {
			this.onDoAction();
		}
	}*/
}
