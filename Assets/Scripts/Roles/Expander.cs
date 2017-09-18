using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Expander : Role {
	public Expander(Citizen citizen): base(citizen){
	}

	internal override void Initialize(GameEvent gameEvent){
		if(gameEvent is Expansion){
            base.Initialize(gameEvent);
//			this.expansion.expander = this;
//			this.targetLocation = this.expansion.hexTileToExpandTo;
			//this.path = path;
//			this.daysBeforeMoving = this.path [0].movementDays;
			this.avatar.GetComponent<ExpansionAvatar>().Init(this);
		}
	}
}
