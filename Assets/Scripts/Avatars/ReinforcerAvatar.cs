using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class ReinforcerAvatar : MonoBehaviour {
	public Reinforcer reinforcer;
	public PandaBehaviour pandaBehaviour;
	public Animator animator;
	public SpriteRenderer kingdomIndicator;
	public TextMesh txtDamage;
	public bool collidedWithHostile;
	public General otherGeneral;


	private bool hasArrived = false;
	private List<HexTile> pathToUnhighlight = new List<HexTile> ();

	internal DIRECTION direction;

	internal void Init(Reinforcer reinforcer){
		this.reinforcer = reinforcer;
		this.kingdomIndicator.color = reinforcer.citizen.city.kingdom.kingdomColor;
		this.txtDamage.text = reinforcer.reinforcementValue.ToString ();
		this.direction = DIRECTION.LEFT;
		this.GetComponent<Avatar> ().kingdom = this.reinforcer.citizen.city.kingdom;
		this.GetComponent<Avatar> ().gameEvent = this.reinforcer.reinforcement;
		this.GetComponent<Avatar> ().citizen = this.reinforcer.citizen;

		ResetValues ();
		this.AddBehaviourTree ();
	}
//	void OnTriggerEnter2D(Collider2D other){
//		if(other.tag == "General"){
//			this.collidedWithHostile = false;
//			if(this.gameObject != null && other.gameObject != null){
//				if(other.gameObject.GetComponent<GeneralAvatar>().general.citizen.city.kingdom.id != this.reinforcer.citizen.city.kingdom.id){
//					if(!other.gameObject.GetComponent<GeneralAvatar> ().general.citizen.isDead){
//						this.collidedWithHostile = true;
//						this.otherGeneral = other.gameObject.GetComponent<GeneralAvatar> ().general;
//					}
//				}
//			}
//		}
//	}
	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
		if(startTile.transform.position.x <= targetTile.transform.position.x){
			if(this.animator.gameObject.transform.localScale.x > 0){
				this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
			}
		}else{
			if(this.animator.gameObject.transform.localScale.x < 0){
				this.animator.gameObject.transform.localScale = new Vector3(this.animator.gameObject.transform.localScale.x * -1, this.animator.gameObject.transform.localScale.y, this.animator.gameObject.transform.localScale.z);
			}
		}
		if(startTile.transform.position.y < targetTile.transform.position.y){
			this.direction = DIRECTION.UP;
			this.animator.Play("Walk_Up");
		}else if(startTile.transform.position.y > targetTile.transform.position.y){
			this.direction = DIRECTION.DOWN;
			this.animator.Play("Walk_Down");
		}else{
			if(startTile.transform.position.x < targetTile.transform.position.x){
				this.direction = DIRECTION.RIGHT;
				this.animator.Play("Walk_Right");
			}else{
				this.direction = DIRECTION.LEFT;
				this.animator.Play("Walk_Left");
			}
		}
		this.GetComponent<SmoothMovement>().direction = this.direction;
		this.GetComponent<SmoothMovement>().Move(targetTile.transform.position);
	}
	private void StopMoving(){
		this.animator.Play("Idle");
	}
	[Task]
	public void IsThereCitizen(){
		if(this.reinforcer.citizen != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void IsThereEvent(){
		if(this.reinforcer.citizen != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void HasArrivedAtTargetHextile(){
		if(this.reinforcer.location == this.reinforcer.targetLocation){
			if(!this.hasArrived){
				this.hasArrived = true;
				this.reinforcer.Attack ();
			}
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}

	}

//	[Task]
//	public void HasCollidedWithHostileGeneral(){
//		if(this.collidedWithHostile){
//			this.collidedWithHostile = false;
//			if(!this.otherGeneral.citizen.isDead){
//				
//				Task.current.Succeed ();
//			}else{
//				Task.current.Fail ();
//			}
//
//		}else{
//			Task.current.Fail ();
//		}
//	}
	[Task]
	public void HasDiedOfOtherReasons(){
		if (this.reinforcer.citizen.isDead) {
			//Citizen has died
			this.reinforcer.reinforcement.DeathByOtherReasons ();
			Task.current.Succeed();
		}else {
			Task.current.Fail ();
		}
	}
	[Task]
	public void MoveToNextTile(){
		Move ();
		Task.current.Succeed ();
	}

	private void ResetValues(){
		this.collidedWithHostile = false;
		this.otherGeneral = null;
	}

	private void Move(){
		if(this.reinforcer.targetLocation != null){
			if(this.reinforcer.path != null){
				if(this.reinforcer.path.Count > 0){
					if(this.reinforcer.daysBeforeMoving <= 0){
						this.MakeCitizenMove (this.reinforcer.location, this.reinforcer.path [0]);
						this.reinforcer.daysBeforeMoving = this.reinforcer.path [0].movementDays;
						this.reinforcer.location = this.reinforcer.path[0];
						this.reinforcer.citizen.currentLocation = this.reinforcer.path [0];
						this.reinforcer.path.RemoveAt (0);
                        this.CheckForKingdomDiscovery();
					}
					this.reinforcer.daysBeforeMoving -= 1;
				}
			}
		}
	}

    private void CheckForKingdomDiscovery() {
        if (this.reinforcer.location.ownedByCity != null &&
            this.reinforcer.location.ownedByCity.kingdom.id != this.reinforcer.citizen.city.kingdom.id) {
            Kingdom thisKingdom = this.reinforcer.citizen.city.kingdom;
            Kingdom otherKingdom = this.reinforcer.location.ownedByCity.kingdom;
            thisKingdom.DiscoverKingdom(otherKingdom);
        }
    }

    internal void AddBehaviourTree(){
		BehaviourTreeManager.Instance.allTrees.Add (this.pandaBehaviour);
	}

	internal void RemoveBehaviourTree(){
		BehaviourTreeManager.Instance.allTrees.Remove (this.pandaBehaviour);
	}


	void OnMouseEnter(){
		if (!UIManager.Instance.IsMouseOnUI()) {
			UIManager.Instance.ShowSmallInfo (this.reinforcer.reinforcement.eventType.ToString ());
			this.HighlightPath ();
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo ();
		this.UnHighlightPath ();
	}

	void HighlightPath(){
		this.pathToUnhighlight.Clear ();
		for (int i = 0; i < this.reinforcer.path.Count; i++) {
			this.reinforcer.path [i].highlightGO.SetActive (true);
			this.pathToUnhighlight.Add (this.reinforcer.path [i]);
		}
	}

	void UnHighlightPath(){
		for (int i = 0; i < this.pathToUnhighlight.Count; i++) {
			this.pathToUnhighlight[i].highlightGO.SetActive(false);
		}
	}

	void OnDestroy(){
		BehaviourTreeManager.Instance.allTrees.Remove (this.pandaBehaviour);
		UnHighlightPath ();
	}

	public void OnEndAttack(){
		this.reinforcer.reinforcement.DoneCitizenAction(this.reinforcer.citizen);
		this.reinforcer.citizen.Death (DEATH_REASONS.ACCIDENT);
	}

	internal void HasAttacked(){
		this.GetComponent<SmoothMovement> ().hasAttacked = true;
	}

}
