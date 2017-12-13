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

		private int	_level;
		private int	_currentHP;
		private int _currentRow;

		private bool _isDead;
		private List<Trait>	_traits;
		private List<BodyPart> _bodyParts;
		private CharacterClass _characterClass;

		#region getters / setters
		internal string name{
			get { return this._name; }
		}
		internal int level{
			get { return this._level; }
		}
		internal int currentHP{
			get { return this._currentHP; }
		}
		internal CharacterClass characterClass{
			get { return this._characterClass; }
		}
		internal List<BodyPart> bodyParts{
			get { return this._bodyParts; }
		}
		internal List<Trait> traits{
			get { return this._traits; }
		}
		internal int currentRow{
			get { return this._currentRow; }
		}
		internal bool isDead{
			get { return this._isDead; }
		}
		#endregion
		//Initializes Character Class
		internal void Init(){
			BodyPartsData bodyPartsData = JsonUtility.FromJson<BodyPartsData> (bodyPartsJson.text);
			this._bodyParts = new List<BodyPart> (bodyPartsData.bodyParts);
			this._characterClass = new CharacterClass ();
			this._characterClass = JsonUtility.FromJson<CharacterClass> (characterClassJson.text);
		}

		//Check if the body parts of this character has the attribute necessary and quantity
		internal bool HasAttribute(IBodyPart.ATTRIBUTE attribute, int quantity){
			int count = 0;
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];

				if(bodyPart.status.Count <= 0){
					if(bodyPart.attributes.Contains(attribute)){
						count += 1;
						if(count >= quantity){
							return true;
						}
					}
				}

				for (int j = 0; j < bodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = bodyPart.secondaryBodyParts [j];
					if (secondaryBodyPart.status.Count <= 0) {
						if (secondaryBodyPart.attributes.Contains (attribute)) {
							count += 1;
							if (count >= quantity) {
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		//Enables or Disables skills based on skill requirements
		internal void EnableDisableSkills(){
			for (int i = 0; i < this._characterClass.skills.Count; i++) {
				Skill skill = this._characterClass.skills [i];
				skill.isEnabled = true;
				for (int j = 0; j < skill.skillRequirements.Length; j++) {
					SkillRequirement skillRequirement = skill.skillRequirements [j];
					if(!HasAttribute(skillRequirement.attributeRequired, skillRequirement.itemQuantity)){
						skill.isEnabled = false;
						break;
					}
				}
			}
		}

		//Changes row number of this character
		internal void SetRowNumber(int rowNumber){
			this._currentRow = rowNumber;
		}

		//Adjust current HP based on specified paramater, but HP must not go below 0
		internal void AdjustHP(int amount){
			this._currentHP += amount;
			if(this._currentHP < 0){
				this._currentHP = 0;
				Death ();
			}
		}

		//Character's death
		internal void Death(){
			this._isDead = true;
			if(Messenger.eventTable.ContainsKey("CharacterDeath")){
				Messenger.Broadcast ("CharacterDeath", this);
			}
		}
	}
}

