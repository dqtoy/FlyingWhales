using UnityEngine;
using System.Collections;

public class AvatarAnimationEvents : MonoBehaviour {

	public GameObject avatarGO;

	public void OnEndAttack(){
		Role role = this.avatarGO.GetComponent<Avatar> ().citizen.assignedRole;
		if(role is General){
			this.avatarGO.GetComponent<GeneralAvatar> ().OnEndAttack ();
		}
	}
}
