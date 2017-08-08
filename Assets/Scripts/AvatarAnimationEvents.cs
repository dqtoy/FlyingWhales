using UnityEngine;
using System.Collections;

public class AvatarAnimationEvents : MonoBehaviour {

	public GameObject avatarGO;

	public void OnEndAttack(){
		if(this.avatarGO.GetComponent<Avatar> () != null){
			this.avatarGO.GetComponent<CitizenAvatar> ().EndAttack ();
//			if(role is General){
//				this.avatarGO.GetComponent<GeneralAvatar> ().OnEndAttack ();
//			}
		}else if(this.avatarGO.GetComponent<MonsterAvatar> () != null){
			this.avatarGO.GetComponent<MonsterAvatar> ().OnEndAttack();
		}
	}
}
