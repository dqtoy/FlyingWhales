using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class ExpansionAvatar : MonoBehaviour {
	public Expander expander;
	public PandaBehaviour pandaBehaviour;
	public Animator animator;
	public bool collidedWithHostile;
	public General otherGeneral;

	private bool hasArrived = false;
//	private bool isMoving = false;
//	private Vector3 targetPosition = Vector3.zero;
	private List<HexTile> pathToUnhighlight = new List<HexTile> ();
	internal DIRECTION direction;

//	public float speed;

//	void Update(){
//		if(this.isMoving){
//			if(this.targetPosition != null){
//				float step = speed * Time.deltaTime;
//				this.transform.position = Vector3.MoveTowards (this.transform.position, this.targetPosition, step);
//				if(Vector3.Distance(this.transform.position, this.targetPosition) < 0.1f){
//					StopMoving ();
//				}
//			}
//		}
//	}
	internal void Init(Expander expander){
		this.expander = expander;
		this.direction = DIRECTION.LEFT;
		this.GetComponent<Avatar> ().kingdom = this.expander.citizen.city.kingdom;
		this.GetComponent<Avatar> ().gameEvent = this.expander.expansion;
		this.GetComponent<Avatar> ().citizen = this.expander.citizen;

		ResetValues ();
		this.AddBehaviourTree ();
	}
//	void OnTriggerEnter2D(Collider2D other){
//		if(other.tag == "General"){
//			this.collidedWithHostile = false;
//			if(this.gameObject != null && other.gameObject != null){
//				if(other.gameObject.GetComponent<GeneralAvatar>().general.citizen.city.kingdom.id != this.expander.citizen.city.kingdom.id){
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
//		this.targetPosition = targetTile.transform.position;
//		this.UpdateUI ();
//		this.isMoving = true;
	}
	private void StopMoving(){
//		this.generalAnimator.Play("Idle");
//		this.isMoving = false;
//		this.targetPosition = Vector3.zero;
	}
//	internal void UpdateUI(){
//		if(this.general != null){
//			this.textMesh.text = this.general.army.hp.ToString ();
//		}
//	}
	[Task]
	public void IsThereCitizen(){
		if(this.expander.citizen != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void IsThereEvent(){
		if(this.expander.expansion != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void HasArrivedAtTargetHextile(){
		if(this.expander.location == this.expander.targetLocation){
			if(!this.hasArrived){
				this.hasArrived = true;
				//Expand to target hextile
				this.expander.expansion.DoneCitizenAction(this.expander.citizen);
				this.expander.DestroyGO();
			}
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}

	}
	[Task]
	public void HasDisappeared(){
		if (!this.expander.location.isOccupied) {
			float chance = UnityEngine.Random.Range (0f, 99f);
			if(chance <= 0.5f){
				//Disappearance
				this.expander.expansion.Disappearance ();
				Task.current.Succeed ();
			}else{
				Task.current.Fail ();
			}
		}else{
			Task.current.Fail ();
		}
	}
		
//	[Task]
//	public void HasCollidedWithHostileGeneral(){
//		if(this.collidedWithHostile){
//			this.collidedWithHostile = false;
//			if(!this.otherGeneral.citizen.isDead){
//				//Death by general
//				this.expander.expansion.DeathByGeneral (this.otherGeneral);
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
		if (this.expander.citizen.isDead) {
			//Citizen has died
			this.expander.expansion.DeathByOtherReasons ();
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
		if(this.expander.targetLocation != null){
			if(this.expander.path != null){
				if(this.expander.path.Count > 0){
					if(this.expander.daysBeforeMoving <= 0){
						this.MakeCitizenMove (this.expander.location, this.expander.path [0]);
						this.expander.daysBeforeMoving = this.expander.path [0].movementDays;
						this.expander.location = this.expander.path[0];
						this.expander.citizen.currentLocation = this.expander.path [0];
						this.expander.path.RemoveAt (0);
                        this.CheckForKingdomDiscovery();
                    }
					this.expander.daysBeforeMoving -= 1;
				}
			}
		}
	}

    private void CheckForKingdomDiscovery() {
        if (this.envoy.location.ownedByCity != null &&
            this.envoy.location.ownedByCity.kingdom.id != this.envoy.citizen.city.kingdom.id) {
            Kingdom thisKingdom = this.envoy.citizen.city.kingdom;
            Kingdom otherKingdom = this.envoy.location.ownedByCity.kingdom;
            thisKingdom.DiscoverKingdom(otherKingdom);
            otherKingdom.DiscoverKingdom(thisKingdom);
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
			UIManager.Instance.ShowSmallInfo (this.expander.expansion.eventType.ToString ());
			this.HighlightPath ();
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo ();
		this.UnHighlightPath ();
	}

	void HighlightPath(){
		this.pathToUnhighlight.Clear ();
		for (int i = 0; i < this.expander.path.Count; i++) {
			this.expander.path [i].highlightGO.SetActive (true);
			this.pathToUnhighlight.Add (this.expander.path [i]);
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
//	private string CampaignInfo(Campaign campaign){
//		string info = string.Empty;
//		info += "id: " + campaign.id;
//		info += "\n";
//
//		info += "campaign type: " + campaign.campaignType.ToString ();
//		info += "\n";
//
//		info += "general: " + this.general.citizen.name;
//		info += "\n";
//
//		info += "target city: " + campaign.targetCity.name;
//		info += "\n";
//		if (campaign.rallyPoint == null) {
//			info += "rally point: N/A";
//		} else {
//			info += "rally point: " + campaign.rallyPoint.name; 
//		}
//		info += "\n";
//
//		info += "leader: " + campaign.leader.name;
//		info += "\n";
//
//		info += "war type: " + campaign.warType.ToString ();
//		info += "\n";
//
//		info += "needed army: " + campaign.neededArmyStrength.ToString ();
//		info += "\n";
//
//		info += "army: " + campaign.GetArmyStrength ().ToString ();
//		info += "\n";
//
//		if (campaign.campaignType == CAMPAIGN.DEFENSE) {
//			if (campaign.expiration == -1) {
//				info += "expiration: none";
//			} else {
//				info += "will expire in " + campaign.expiration + " days";
//			}
//		} else {
//			info += "expiration: none";
//
//		}
//
//		return info;
//	}


}
