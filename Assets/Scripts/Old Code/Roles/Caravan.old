using UnityEngine;
using System.Collections;

public class Caravan : Role {
	
	internal bool isActivated;
	public Caravan(Citizen citizen): base(citizen){
		this.role = ROLE.CARAVAN;
	}

	internal override void Initialize(GameEvent gameEvent){
		base.Initialize(gameEvent);
		this.avatar.GetComponent<CaravanAvatar>().Init(this);
	}

	internal void CaravanActivation(bool state){
		this.isActivated = state;
		this.avatar.SetActive (state);
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
