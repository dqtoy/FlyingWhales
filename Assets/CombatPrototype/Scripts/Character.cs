using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Character: MonoBehaviour {
		[SerializeField] private int _maxHP;
		public TextAsset characterClassJson;
		public TextAsset bodyPartsJson;

		private string _name;
		private List<Trait>	_traits;
		private int	_level;
		private int	_currentHP;
		private List<BodyPart> _bodyParts;
		private CharacterClass _characterClass;


		void Start(){
			BodyPartsData bodyPartsData = JsonUtility.FromJson<BodyPartsData> (bodyPartsJson.text);
			this._bodyParts = new List<BodyPart> (bodyPartsData.bodyParts);
			this._characterClass = new CharacterClass ();
			this._characterClass = JsonUtility.FromJson<CharacterClass> (characterClassJson.text);
		}
	}
}

