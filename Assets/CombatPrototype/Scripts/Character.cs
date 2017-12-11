using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	[System.Serializable]
	public class Character: EntityComponent {
		[SerializeField] public int _maxHP;
		[SerializeField] public TextAsset characterClassJson;
		[SerializeField] public TextAsset bodyPartsJson;

		private string _name;
		private List<Trait>	_traits;
		private int	_level;
		private int	_currentHP;
		private List<BodyPart> _bodyParts;
		private CharacterClass _characterClass;


		//Initializes Character Class
		internal void Init(){
			BodyPartsData bodyPartsData = JsonUtility.FromJson<BodyPartsData> (bodyPartsJson.text);
			this._bodyParts = new List<BodyPart> (bodyPartsData.bodyParts);
			this._characterClass = new CharacterClass ();
			this._characterClass = JsonUtility.FromJson<CharacterClass> (characterClassJson.text);
		}
	}
}

