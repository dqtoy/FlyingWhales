using UnityEngine;
using System.Collections;

public class AvatarAnimationEvents : MonoBehaviour {

	public GameObject avatarGO;

	public void OnEndAttack(){
		if(this.avatarGO.GetComponent<Avatar> () != null){
			Role role = this.avatarGO.GetComponent<Avatar> ().citizen.assignedRole;
			if(role is General){
				this.avatarGO.GetComponent<GeneralAvatar> ().OnEndAttack ();
			}
		}else if(this.avatarGO.GetComponent<MonsterAvatar> () != null){
			Monster monster = this.avatarGO.GetComponent<MonsterAvatar> ().monster;
			if(monster is Lycan){
				this.avatarGO.GetComponent<LycanAvatar> ().OnEndAttack ();
			}if(monster is StormWitch){
				this.avatarGO.GetComponent<StormWitchAvatar> ().OnEndAttack ();
			}
		}

	}
}
