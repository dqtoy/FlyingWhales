using UnityEngine;
using System.Collections;

namespace ECS{
	public class CharacterSummary : MonoBehaviour {
		public CharacterSummary Instance;

		public UILabel basicInfoLbl;
		public UILabel classInfoLbl;
		public UILabel bodyPartsInfoLbl;

		void Awake(){
			Instance = this;
		}

		public void ShowCharacterSummary(Character character){
			

		}

//		private void BasicInfo(Character character){
//			summaryLbl 
//		}

	}
}

