using UnityEngine;
using System.Collections;

public class StormWitch : Monster {

	public StormWitch (MONSTER type, HexTile originHextile) : base (type, originHextile){
		Initialize();
	}


	#region Overrides
//	internal override void Attack (){
//		if(this.avatar != null){
//			this.avatar.GetComponent<StormWitchAvatar> ().HasAttacked();
//			if(this.avatar.GetComponent<StormWitchAvatar> ().direction == DIRECTION.LEFT){
//				this.avatar.GetComponent<StormWitchAvatar> ().animator.Play ("Attack_Left");
//			}else if(this.avatar.GetComponent<StormWitchAvatar> ().direction == DIRECTION.RIGHT){
//				this.avatar.GetComponent<StormWitchAvatar> ().animator.Play ("Attack_Right");
//			}else if(this.avatar.GetComponent<StormWitchAvatar> ().direction == DIRECTION.UP){
//				this.avatar.GetComponent<StormWitchAvatar> ().animator.Play ("Attack_Up");
//			}else{
//				this.avatar.GetComponent<StormWitchAvatar> ().animator.Play ("Attack_Down");
//			}
//		}
//	}
	internal override void Initialize(){
        CreateAvatarGO();
		this.avatar.GetComponent<StormWitchAvatar>().Init(this);
	}
	internal override void Death (){
		base.Death ();
		if(this.avatar != null){
            ObjectPoolManager.Instance.DestroyObject(this.avatar);
            this.avatar = null;
		}
	}
	internal override void DoneAction (){
		base.DoneAction ();
		this.Death();
		if(this.targetLocation.isOccupied && this.targetLocation.isHabitable && (this.targetLocation.city != null && this.targetLocation.city.id != 0 && !this.targetLocation.city.isDead)){
			//EventCreator.Instance.CreateGreatStormEvent (this.targetLocation.city.kingdom);
			this.targetLocation.city.RetaliateToMonster (this.originHextile);
		}

	}
	#endregion
}
