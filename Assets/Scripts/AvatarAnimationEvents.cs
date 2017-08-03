using UnityEngine;
using System.Collections;

public class AvatarAnimationEvents : MonoBehaviour {

//	private GameObject avatarGO;

	public void OnEndAttack(){
		if(this.gameObject.GetComponent<Avatar> () != null){
			this.gameObject.GetComponent<CitizenAvatar> ().EndAttack ();
//			if(role is General){
//				this.avatarGO.GetComponent<GeneralAvatar> ().OnEndAttack ();
//			}
		}else if(this.gameObject.GetComponent<MonsterAvatar> () != null){
			Monster monster = this.gameObject.GetComponent<MonsterAvatar> ().monster;
			if(monster is Lycan){
				this.gameObject.GetComponent<LycanAvatar> ().OnEndAttack ();
			}if(monster is StormWitch){
				this.gameObject.GetComponent<StormWitchAvatar> ().OnEndAttack ();
			}
		}

	}
}
