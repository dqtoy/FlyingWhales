using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class GeneralAvatar : CitizenAvatar {
	public UILabel txtDamage;
	public UI2DSprite kingdomIndicator;

	#region Overrides
	internal override void Init (Role citizenRole){
		base.Init (citizenRole);
		this.kingdomIndicator.color = this.citizenRole.citizen.city.kingdom.kingdomColor;
	}
	internal override void HasArrivedAtTargetLocation (){
		if (this.citizenRole.location == this.citizenRole.targetLocation) {
			if (!this.hasArrived) {
				SetHasArrivedState(true);
				this.citizenRole.Attack ();
			}
		}
	}
	internal override void UpdateUI (){
		if(this.txtDamage.gameObject != null){
			this.txtDamage.text = ((General)this.citizenRole).soldiers.ToString ();
		}
	}

	#endregion
}
