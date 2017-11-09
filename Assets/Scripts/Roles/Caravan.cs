using UnityEngine;
using System.Collections;

public class Caravan : Role {

	public Caravan(Citizen citizen): base(citizen){
	
	}

	internal override void Initialize(GameEvent gameEvent){
		base.Initialize(gameEvent);
		this.avatar.GetComponent<CaravanAvatar>().Init(this);
	}

//	internal override void CreateAvatarGO (){
//		this.avatar = (GameObject)GameObject.Instantiate (ObjectPoolManager.Instance.citizenAvatarPrefabs [2], this.citizen.assignedRole.location.transform);
//		this.avatar.transform.localPosition = Vector3.zero;
//	}
//	internal override void DestroyGO(){
//		this.location.ExitCitizen(this.citizen);
//		if (this.avatar != null){
//			UIManager.Instance.HideSmallInfo ();
//			GameObject.Destroy (this.avatar);
//			this.avatar = null;
//		}
//		this.isDestroyed = true;
//	}
}
