using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Role {
	public Citizen citizen;

    private int _hp;
    private int _maxHP;

	public List<HexTile> path;
	public HexTile location;
	public HexTile targetLocation;
	public City targetCity;

	public GameObject avatar;
	public int daysBeforeMoving;
	public bool isDestroyed;
	public bool hasAttacked;

	public int damage;
	public bool markAsDead;


    private GameEvent _gameEventInvolvedIn;

    private Plague _plague;

    #region getters/setters
    public Plague plague {
        get { return this._plague; }
    }
    public GameEvent gameEventInvolvedIn {
        get { return this._gameEventInvolvedIn; }
    }
    #endregion

    public Role(Citizen citizen){
		this.citizen = citizen;
		this.location = citizen.city.hexTile;
		this.targetLocation = null;
		this.targetCity = null;
		this.path = new List<HexTile> ();
		this.avatar = null;
		this.daysBeforeMoving = 0;
		this.isDestroyed = false;
		this.hasAttacked = false;
		this.damage = 1;
		this.markAsDead = false;
	}
	internal void DestroyGO(){
        this.location.ExitCitizen(this.citizen);
        if (this.avatar != null){
			UIManager.Instance.HideSmallInfo ();
            ObjectPoolManager.Instance.DestroyObject(this.avatar);
            this.avatar = null;
        }
		this.isDestroyed = true;
	}

	#region Virtuals
	internal virtual int[] GetResourceProduction(){
		int goldProduction = 40;
		//		if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.LAVISH)) {
		//			goldProduction -= 5;
		//		} else if (this.citizen.city.governor.skillTraits.Contains (SKILL_TRAIT.THRIFTY)) {
		//			goldProduction += 5;
		//		}
		//
		//		if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.LAVISH)) {
		//			goldProduction -= 5;
		//		} else if (this.citizen.city.kingdom.king.skillTraits.Contains (SKILL_TRAIT.THRIFTY)) {
		//			goldProduction += 5;
		//		}
		return new int[]{ 0, 0, 0, 0, 0, 0, goldProduction, 0 };
	}
	internal virtual void OnDeath(){
        //if (this.avatar != null) {
        //    this.avatar.GetComponent<CitizenAvatar>().UpdateFogOfWar(true);
        //}

        //this.location.ExitCitizen(this.citizen);
        this.DisinfectPlague();
		this.DestroyGO ();
	}
	internal virtual void Initialize(GameEvent gameEvent){
        SetGameEventInvolvedIn(gameEvent);
        CheckForPlagueInfection();
        //if (ObjectPoolManager.Instance.HasPool(citizen.role.ToString())){
            CreateAvatarGO();
        //}
    }

    internal void CreateAvatarGO() {
        this.avatar = ObjectPoolManager.Instance.InstantiateObjectFromPool(citizen.role.ToString(), Vector3.zero, Quaternion.identity,
            this.citizen.city.hexTile.transform);
    }

	internal virtual void Attack(){
		if (this.avatar != null) {
			if(this.avatar.GetComponent<CitizenAvatar>().animator.gameObject.activeSelf){
				if (this.avatar.GetComponent<CitizenAvatar>().direction == DIRECTION.LEFT) {
					this.avatar.GetComponent<CitizenAvatar>().animator.Play("Attack_Left");
				} else if (this.avatar.GetComponent<CitizenAvatar>().direction == DIRECTION.RIGHT) {
					this.avatar.GetComponent<CitizenAvatar>().animator.Play("Attack_Right");
				} else if (this.avatar.GetComponent<CitizenAvatar>().direction == DIRECTION.UP) {
					this.avatar.GetComponent<CitizenAvatar>().animator.Play("Attack_Up");
				} else {
					this.avatar.GetComponent<CitizenAvatar>().animator.Play("Attack_Down");
				}
			}else{
				this.avatar.GetComponent<CitizenAvatar> ().EndAttack ();
			}
		}
	}

	#endregion

   /*
    * Whenever an agent comes out of a plagued city, 
    * there is a 5% chance for every plagued settlement that he carries the plague
    * */
    protected void CheckForPlagueInfection() {
        if (this.citizen.city.plague != null) {
            int numOfInfectedSettlements = this.citizen.city.plaguedSettlements.Count;
            int chanceToPlague = 10 * numOfInfectedSettlements;
            if (Random.Range(0, 100) < chanceToPlague) {
                this.InfectWithPlague(this.citizen.city.plague);
            }
        }
    }

    protected void InfectWithPlague(Plague plague) {
        this._plague = plague;
    }

    internal void DisinfectPlague() {
        this._plague = null;
    }

    internal void SetGameEventInvolvedIn(GameEvent _gameEventInvolvedIn) {
        this._gameEventInvolvedIn = _gameEventInvolvedIn;
    }
    
    internal void UpdateUI(){
		if(this.avatar != null){
			if(this is General){
				this.avatar.GetComponent<GeneralAvatar>().UpdateUI();
			}
		}
	}
}
