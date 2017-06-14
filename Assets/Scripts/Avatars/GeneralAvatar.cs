using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class GeneralAvatar : MonoBehaviour {
	public General general;
	public PandaBehaviour pandaBehaviour;
	public Animator animator;
	public SpriteRenderer kingdomIndicator;
	public TextMesh txtDamage;
	public bool collidedWithHostile;
	public Citizen hostile;


	private bool hasArrived = false;
	//	private bool isMoving = false;
	//	private Vector3 targetPosition = Vector3.zero;
	private List<HexTile> pathToUnhighlight = new List<HexTile> ();

	//	public float speed;
	internal DIRECTION direction;
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
	internal void Init(General general){
		this.general = general;
		this.kingdomIndicator.color = general.citizen.city.kingdom.kingdomColor;
		this.txtDamage.text = general.damage.ToString ();
		this.direction = DIRECTION.LEFT;
		this.GetComponent<Avatar> ().kingdom = this.general.citizen.city.kingdom;
		this.GetComponent<Avatar> ().gameEvent = this.general.attackCity;
		this.GetComponent<Avatar> ().citizen = this.general.citizen;

		ResetValues ();
		this.AddBehaviourTree ();
	}
	void OnTriggerEnter2D(Collider2D other){
		if (other.tag == "General") {
			this.collidedWithHostile = false;
			if (this.gameObject != null && other.gameObject != null) {
				if (other.gameObject.GetComponent<Avatar> ().kingdom.id != this.general.citizen.city.kingdom.id) {
					if (!other.gameObject.GetComponent<Avatar> ().citizen.isDead) {
						this.collidedWithHostile = true;
						this.hostile = other.gameObject.GetComponent<Avatar>().citizen;
						HasCollidedWithHostile ();
					}
				}else{
					bool isRebel = ((General)other.gameObject.GetComponent<Avatar> ().citizen.assignedRole).isRebel;
					if(isRebel && this.general.isRebel == !isRebel){
						if (!other.gameObject.GetComponent<Avatar> ().citizen.isDead) {
							this.collidedWithHostile = true;
							this.hostile = other.gameObject.GetComponent<Avatar>().citizen;
							HasCollidedWithHostile ();
						}
					}
				}
			}
		} else if(other.tag == "Trader"){
			this.collidedWithHostile = false;
			if(this.gameObject != null && other.gameObject != null){
				Kingdom kingdomOfGeneral = this.gameObject.GetComponent<GeneralAvatar>().general.citizen.city.kingdom;
				Kingdom kingdomOfTrader = other.gameObject.GetComponent<Avatar>().kingdom;
				if (kingdomOfGeneral.id != kingdomOfTrader.id) {
					RelationshipKings relOfGeneralWithTrader = kingdomOfGeneral.king.GetRelationshipWithCitizen(kingdomOfTrader.king);
					RelationshipKings relOfTraderWithGeneral = kingdomOfTrader.king.GetRelationshipWithCitizen(kingdomOfGeneral.king);
					if (relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfGeneralWithTrader.lordRelationship == RELATIONSHIP_STATUS.RIVAL ||
						relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relOfTraderWithGeneral.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
						if (!other.gameObject.GetComponent<Avatar>().citizen.isDead) {
							this.collidedWithHostile = true;
							this.hostile = other.gameObject.GetComponent<Avatar>().citizen;
							HasCollidedWithHostile ();
						}
					}  
				}
			}
		} else if(other.tag == "Avatar"){
			this.collidedWithHostile = false;
			if(this.gameObject != null && other.gameObject != null){
				if(other.gameObject.GetComponent<Avatar>().kingdom.id != this.general.citizen.city.kingdom.id){
					if(!other.gameObject.GetComponent<Avatar> ().citizen.isDead){
						this.collidedWithHostile = true;
						this.hostile = other.gameObject.GetComponent<Avatar>().citizen;
						HasCollidedWithHostile ();
					}
				}
			}
		}


	}
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
		this.GetComponent<SmoothMovement>().direction = this.direction;
		this.GetComponent<SmoothMovement>().Move(targetTile.transform.position);
		//		this.targetPosition = targetTile.transform.position;
		this.UpdateUI ();
		//		this.isMoving = true;
	}
	private void StopMoving(){
		this.animator.Play("Idle");
		//		this.isMoving = false;
		//		this.targetPosition = Vector3.zero;
	}
	internal void UpdateUI(){
		if(this.general != null){
			this.txtDamage.text = this.general.damage.ToString ();
		}
	}
	[Task]
	public void IsThereCitizen(){
		if(this.general.citizen != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}
	[Task]
	public void IsThereEvent(){
		if(this.general.citizen != null){
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}
	}

	[Task]
	public void HasArrivedAtTargetHextile(){
		if(this.general.location == this.general.targetLocation){
			if(!this.hasArrived){
				this.hasArrived = true;
				this.GetComponent<BoxCollider2D> ().enabled = false;
				this.general.Attack ();
			}
			Task.current.Succeed ();
		}else{
			Task.current.Fail ();
		}

	}

//	[Task]
	public void HasCollidedWithHostile(){
		if(this.collidedWithHostile){
			this.collidedWithHostile = false;
			if(this.hostile.assignedRole != null){
				if(this.hostile.assignedRole is General){
					if(!this.hostile.isDead){
						General otherGeneral = (General)this.hostile.assignedRole;
						CombatManager.Instance.BattleMidway (ref this.general, ref otherGeneral);
						if(this.general.markAsDead){
							this.general.citizen.Death (DEATH_REASONS.BATTLE);
						}else{
							this.general.avatar.GetComponent<GeneralAvatar> ().UpdateUI ();
						}
						if(otherGeneral.markAsDead){
							this.hostile.Death (DEATH_REASONS.BATTLE);
						}else{
							otherGeneral.avatar.GetComponent<GeneralAvatar> ().UpdateUI ();
						}
					}
				}else{
					if (!this.hostile.isDead) {
						this.hostile.assignedRole.avatar.GetComponent<Avatar> ().gameEvent.DeathByGeneral (this.general);
					}
				}
//				Task.current.Succeed ();
			}
//			else{
//				Task.current.Fail ();
//			}
		}
//		else{
//			Task.current.Fail ();
//		}
//		Task.current.Fail ();
	}
	[Task]
	public void HasDiedOfOtherReasons(){
		if (this.general.citizen.isDead) {
			//Citizen has died
			this.general.attackCity.DeathByOtherReasons ();
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
		this.hostile = null;
	}

	private void Move(){
		if(this.general.targetLocation != null){
			if(this.general.path != null){
				if(this.general.path.Count > 0){
					if(this.general.daysBeforeMoving <= 0){
						this.MakeCitizenMove (this.general.location, this.general.path [0]);
						this.general.daysBeforeMoving = this.general.path [0].movementDays;
						this.general.location = this.general.path[0];
						this.general.citizen.currentLocation = this.general.path [0];
						this.general.path.RemoveAt (0);
                        this.CheckForKingdomDiscovery();
					}
					this.general.daysBeforeMoving -= 1;
//					this.MakeCitizenMove (this.general.location, this.general.path [0]);
//					this.raider.daysBeforeMoving = this.raider.path [0].movementDays;
//					this.general.location = this.general.path[0];
//					this.general.citizen.currentLocation = this.general.path [0];
//					this.general.path.RemoveAt (0);
				}
			}
		}
	}

    private void CheckForKingdomDiscovery() {
        if (this.general.location.ownedByCity != null &&
            this.general.location.ownedByCity.kingdom.id != this.general.citizen.city.kingdom.id) {
            Kingdom thisKingdom = this.general.citizen.city.kingdom;
            Kingdom otherKingdom = this.general.location.ownedByCity.kingdom;
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
			UIManager.Instance.ShowSmallInfo (this.general.attackCity.eventType.ToString ());
			this.HighlightPath ();
		}
	}

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo ();
		this.UnHighlightPath ();
	}

	void HighlightPath(){
		this.pathToUnhighlight.Clear ();
		for (int i = 0; i < this.general.path.Count; i++) {
			this.general.path [i].highlightGO.SetActive (true);
			this.pathToUnhighlight.Add (this.general.path [i]);
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
		this.general.attackCity.DoneCitizenAction(this.general.citizen);
	}

	internal void HasAttacked(){
		this.GetComponent<SmoothMovement> ().hasAttacked = true;
	}

}
