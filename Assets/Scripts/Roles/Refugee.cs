using UnityEngine;
using System.Collections;

public class Refugee : Role {

	private int _population;

	#region getters/setters
	public int population{
		get { return this._population; }
	}
	#endregion
	public Refugee(Citizen citizen): base(citizen){
		this.role = ROLE.REFUGEE;
	}

	#region Overrides
	internal override void Initialize(GameEvent gameEvent){
		base.Initialize(gameEvent);
		this.avatar.GetComponent<RefugeeAvatar>().Init(this);
	}
	#endregion

	internal void AdjustPopulation(int amount){
		this._population += amount;
		UpdateUI ();
		if(this._population <= 0){
			DeathOfRefugees ();
		}
	}
	internal void DeathOfRefugees(){
		this._population = 0;
		this.gameEventInvolvedIn.DoneEvent ();
	}
}
