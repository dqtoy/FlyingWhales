using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Invisible : Trait {

        public Character owner { get; private set; }
        public List<Character> charactersThatCanSee { get; private set; }
        public List<Character> inRangeOfVisionCharacters { get; private set; } //Must keep a list of all characters that is in vision of this character but is not added in inVisionPOIs because this character is invisible
        public Invisible() {
            name = "Invisible";
            description = "This character is invisible.";
            type = TRAIT_TYPE.SPECIAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            
            daysDuration = 0;
            //effects = new List<TraitEffect>();
            charactersThatCanSee = new List<Character>();
            inRangeOfVisionCharacters = new List<Character>();
        }

        public void AddCharacterThatCanSee(Character character) {
            if (!charactersThatCanSee.Contains(character)) {
                charactersThatCanSee.Add(character);
                if (RemoveInRangeOfVisionCharacter(character)) {
                    //If a character can see this character and it is in vision range, transfer this character to inVisionPOIs list of the character that can see
                    character.marker.AddPOIAsInVisionRange(owner);
                }
            }
        }
        public void AddInRangeOfVisionCharacter(Character character) {
            if (!inRangeOfVisionCharacters.Contains(character)) {
                inRangeOfVisionCharacters.Add(character);
            }
        }
        public bool RemoveInRangeOfVisionCharacter(Character character) {
            return inRangeOfVisionCharacters.Remove(character);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                //owner.StopAllActionTargettingThis();
                //owner.ForceCancelAllJobsTargettingThisCharacter();
            }
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            if (sourcePOI is Character) {
                GameManager.Instance.StartCoroutine(RetriggerVisionCollision(owner));
            }
        }
        #endregion

        private IEnumerator RetriggerVisionCollision(Character character) {
            for (int i = 0; i < character.marker.colliders.Length; i++) {
                character.marker.colliders[i].enabled = false;
            }
            yield return null;
            for (int i = 0; i < character.marker.colliders.Length; i++) {
                character.marker.colliders[i].enabled = true;
            }
        }
    }

    public class SaveDataInvisible : SaveDataTrait {
        public List<int> charactersThatCanSeeID;
        //public List<int> inRangeOfVisionCharactersID; //This is no longer saved because this is already triggered on entering collision which happens when game is loaded

        public override void Save(Trait trait) {
            base.Save(trait);
            Invisible derivedTrait = trait as Invisible;
            for (int i = 0; i < derivedTrait.charactersThatCanSee.Count; i++) {
                charactersThatCanSeeID.Add(derivedTrait.charactersThatCanSee[i].id);
            }
            //for (int i = 0; i < derivedTrait.inRangeOfVisionCharacters.Count; i++) {
            //    inRangeOfVisionCharactersID.Add(derivedTrait.inRangeOfVisionCharacters[i].id);
            //}
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Invisible derivedTrait = trait as Invisible;
            for (int i = 0; i < charactersThatCanSeeID.Count; i++) {
                derivedTrait.AddCharacterThatCanSee(CharacterManager.Instance.GetCharacterByID(charactersThatCanSeeID[i]));
            }
            return trait;
        }
    }

}
