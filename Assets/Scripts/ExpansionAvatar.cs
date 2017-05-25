using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class ExpansionAvatar : MonoBehaviour {
	public Citizen citizen;
	public PandaBehaviour pandaBehaviour;
	public Animator animator;
	public HexTile location;
	public HexTile targetLocation;
	public List<HexTile> path;
	public bool collidedWithHostile;
	public General otherGeneral;

	private bool hasArrived = false;
	private bool isMoving = false;
	private Vector3 targetPosition = Vector3.zero;
	private int daysBeforeMoving = 0;
	private List<HexTile> pathToUnhighlight = new List<HexTile> ();
	internal Expansion expansionEvent = null;

	public float speed;

	void Update(){
		if(this.isMoving){
			if(this.targetPosition != null){
				float step = speed * Time.deltaTime;
				this.transform.position = Vector3.MoveTowards (this.transform.position, this.targetPosition, step);
				if(Vector3.Distance(this.transform.position, this.targetPosition) < 0.1f){
					StopMoving ();
				}
			}
		}
	}
	internal void Init(Expansion expansionEvent){
		this.citizen = expansionEvent.startedBy;
		this.expansionEvent = expansionEvent;
		this.targetLocation = expansionEvent.hexTileToExpandTo;
		this.location = this.citizen.city.hexTile;
		this.path = new List<HexTile>(expansionEvent.path);
		this.daysBeforeMoving = this.path [0].movementDays;
		ResetValues ();
		this.AddBehaviourTree ();
	}
	void OnTriggerEnter2D(Collider2D other){
		if(other.tag == "General"){
			this.collidedWithHostile = false;
			if(this.gameObject != null && other.gameObject != null){
				if(other.gameObject.GetComponent<GeneralObject>().general.citizen.city.kingdom.id != this.citizen.city.kingdom.id){
					if(other.gameObject.GetComponent<GeneralObject> ().general.army.hp > 0){
						this.collidedWithHostile = true;
						this.otherGeneral = other.gameObject.GetComponent<GeneralObject> ().general;
					}
				}
			}
		}


	}
	internal void MakeCitizenMove(HexTile startTile, HexTile targetTile){
//		this.transform.position = Vector3.MoveTowards (startTile.transform.position, targetTile.transform.position, 0.5f);
//		if(startTile.transform.position.x <= targetTile.transform.position.x){
//			if(this.generalAnimator.gameObject.transform.localScale.x > 0){
//				this.generalAnimator.gameObject.transform.localScale = new Vector3(this.generalAnimator.gameObject.transform.localScale.x * -1, this.generalAnimator.gameObject.transform.localScale.y, this.generalAnimator.gameObject.transform.localScale.z);
//			}
//		}else{
//			if(this.generalAnimator.gameObject.transform.localScale.x < 0){
//				this.generalAnimator.gameObject.transform.localScale = new Vector3(this.generalAnimator.gameObject.transform.localScale.x * -1, this.generalAnimator.gameObject.transform.localScale.y, this.generalAnimator.gameObject.transform.localScale.z);
//			}
//		}
//		if(startTile.transform.position.y < targetTile.transform.position.y){
//			this.generalAnimator.Play("Walk_Up");
//		}else{
//			this.generalAnimator.Play("Walk");
//		}

		this.targetPosition = targetTile.transform.position;
//		this.UpdateUI ();
		this.isMoving = true;
	}
	private void StopMoving(){
//		this.generalAnimator.Play("Idle");
		this.isMoving = false;
		this.targetPosition = Vector3.zero;
	}
//	internal void UpdateUI(){
//		if(this.general != null){
//			this.textMesh.text = this.general.army.hp.ToString ();
//		}
//	}
	[Task]
	public void IsThereCitizen(){
		if(this.citizen != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void IsThereEvent(){
		if(this.expansionEvent != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void HasArrivedAtTargetHextile(){
		if(this.location == this.targetLocation){
			if(!this.hasArrived){
				this.hasArrived = true;
				//Expand to target hextile
				this.expansionEvent.ExpandToTargetHextile();
			}
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}

	}
	[Task]
	public void HasDisappeared(){
		if (!this.location.isOccupied) {
			float chance = UnityEngine.Random.Range (0f, 99f);
			if(chance <= 0.5f){
				//Disappearance
				this.expansionEvent.Disappearance ();
				Task.current.Succeed ();
			}else{
				Task.current.Fail ();
			}
		}else{
			Task.current.Fail ();
		}
	}
		
	[Task]
	public void HasCollidedWithHostileGeneral(){
		if(this.collidedWithHostile){
			this.collidedWithHostile = false;
			if(this.otherGeneral.army.hp > 0){
				//Death by general
				this.expansionEvent.DeathByGeneral ();
				Task.current.Succeed ();
			}else{
				Task.current.Fail ();
			}

		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void HasDiedOfOtherReasons(){
		if (this.citizen.isDead) {
			//Citizen has died
			this.expansionEvent.DeathByOtherReasons ();
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
		if(this.targetLocation != null){
			if(this.path != null){
				if(this.path.Count > 0){
					if(this.daysBeforeMoving <= 0){
						this.MakeCitizenMove (this.location, this.path [0]);
						this.daysBeforeMoving = this.path [0].movementDays;
						this.location = this.path[0];
						this.citizen.currentLocation = this.path [0];
						this.path.RemoveAt (0);
					}
					this.daysBeforeMoving -= 1;
				}
			}
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
			this.HighlightPath ();
		}
	}

	void OnMouseExit(){
		this.UnHighlightPath ();
	}

	void HighlightPath(){
		this.pathToUnhighlight.Clear ();
		for (int i = 0; i < this.path.Count; i++) {
			this.path [i].highlightGO.SetActive (true);
			this.pathToUnhighlight.Add (this.path [i]);
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
