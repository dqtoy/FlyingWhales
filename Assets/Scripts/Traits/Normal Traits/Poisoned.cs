using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Poisoned : Trait {

        public List<Character> awareCharacters { get; private set; } //characters that know about this trait

        public ITraitable traitable { get; private set; } //poi that has the poison
        public Poisoned() {
            name = "Poisoned";
            description = "This object is poisoned.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
            awareCharacters = new List<Character>();
            mutuallyExclusive = new string[] { "Robust" };
            SetLevel(2);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            traitable = sourceCharacter;
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            base.OnRemoveTrait(sourceCharacter, removedBy);
            awareCharacters.Clear();
            responsibleCharacters.Clear(); //Cleared list, for garbage collection
                                           //Messenger.Broadcast(Signals.OLD_NEWS_TRIGGER, sourceCharacter, gainedFromDoing);
        }
        public override string GetTestingData() {
            string summary = string.Empty;
            WeightedDictionary<string> weights = GetResultWeights();
            foreach (KeyValuePair<string, int> kvp in weights.dictionary) {
                summary += "(" + kvp.Key + "-" + kvp.Value.ToString() + ")";
            }
            return summary;
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
            base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                WeightedDictionary<string> result = GetResultWeights();
                string res = result.PickRandomElementGivenWeights();
                if (res == "Sick") {
                    Sick sick = new Sick();
                    for (int i = 0; i < responsibleCharacters.Count; i++) {
                        sick.AddCharacterResponsibleForTrait(responsibleCharacters[i]);
                    }
                    goapNode.actor.traitContainer.AddTrait(goapNode.actor, sick);
                } else { //if (res == "Death")
                    goapNode.actor.Death("poisoned", deathFromAction: goapNode);
                }
                if(traitable is IPointOfInterest) {
                    IPointOfInterest poi = traitable as IPointOfInterest;
                    poi.traitContainer.RemoveTrait(traitable, this);
                    isRemoved = true;
                }
            }
        }
        #endregion

        #region Aware Characters
        public void AddAwareCharacter(Character character) {
            if (awareCharacters.Contains(character)) {
                awareCharacters.Add(character);
            }
        }
        public void RemoveAwareCharacter(Character character) {
            awareCharacters.Remove(character);
        }
        #endregion

        public WeightedDictionary<string> GetResultWeights() {
            WeightedDictionary<string> weights = new WeightedDictionary<string>();
            if (level == 1) {
                weights.AddElement("Sick", 80);
                weights.AddElement("Death", 20);
            } else if (level == 2) {
                weights.AddElement("Sick", 50);
                weights.AddElement("Death", 50);
            } else {
                weights.AddElement("Sick", 20);
                weights.AddElement("Death", 80);
            }
            return weights;
        }
    }

    public class SaveDataPoisoned : SaveDataTrait {
        public List<int> awareCharacterIDs;

        public override void Save(Trait trait) {
            base.Save(trait);
            Poisoned derivedTrait = trait as Poisoned;
            for (int i = 0; i < derivedTrait.awareCharacters.Count; i++) {
                awareCharacterIDs.Add(derivedTrait.awareCharacters[i].id);
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Poisoned derivedTrait = trait as Poisoned;
            for (int i = 0; i < awareCharacterIDs.Count; i++) {
                derivedTrait.AddAwareCharacter(CharacterManager.Instance.GetCharacterByID(awareCharacterIDs[i]));
            }
            return trait;
        }
    }
}
