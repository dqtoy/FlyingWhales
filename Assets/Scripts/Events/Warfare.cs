using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Warfare {
	private int _id;
	private List<Kingdom> _sideA;
	private List<Kingdom> _sideB;
	private List<Battle> _battles;

	public Warfare(Kingdom firstKingdom, Kingdom secondKingdom){
		SetID();
		this._sideA = new List<Kingdom>();
		this._sideB = new List<Kingdom>();
		this._battles = new List<Battle>();
		JoinWar(WAR_SIDE.A, firstKingdom);
		JoinWar(WAR_SIDE.B, secondKingdom);
	}
	private void SetID(){
		this._id = Utilities.lastWarfareID + 1;
		Utilities.lastWarfareID = this._id;
	}

	internal void JoinWar(WAR_SIDE side, Kingdom kingdom){
		if(side == WAR_SIDE.A){
			this._sideA.Add(kingdom);
		}else if(side == WAR_SIDE.B){
			this._sideB.Add(kingdom);
		}
		kingdom.SetWarfareInfo(new WarfareInfo(side, this));
	}
	internal void UnjoinWar(WAR_SIDE side, Kingdom kingdom){
		if(side == WAR_SIDE.A){
			this._sideA.Remove(kingdom);
		}else if(side == WAR_SIDE.B){
			this._sideB.Remove(kingdom);
		}
		kingdom.SetWarfareInfoToDefault();
	}
	internal void BattleEnds(City city){
		
	}
}
