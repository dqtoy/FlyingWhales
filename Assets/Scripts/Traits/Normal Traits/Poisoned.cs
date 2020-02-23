using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Poisoned : Trait {

        public List<Character> awareCharacters { get; } //characters that know about this trait
        private ITraitable traitable { get; set; } //poi that has the poison
        private Character characterOwner;
        private StatusIcon _statusIcon;
        private GameObject _poisonedEffect;

        public Poisoned() {
            name = "Poisoned";
            description = "This object is poisoned.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(4);
            //effects = new List<TraitEffect>();
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
            awareCharacters = new List<Character>();
            mutuallyExclusive = new string[] { "Robust" };
            moodEffect = -12;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            SetLevel(1);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            traitable = addedTo;
            if(traitable is IPointOfInterest) {
                _poisonedEffect = GameManager.Instance.CreateParticleEffectAt(traitable as IPointOfInterest, PARTICLE_EFFECT.Poison, false);
            }
            if(traitable is Character character) {
                characterOwner = character;
                characterOwner.AdjustDoNotRecoverHP(1);
                _statusIcon = character.marker.AddStatusIcon(this.name);
            } else if (addedTo is TileObject tileObject) {
                ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.SetDefaultTileColor(Color.green);
                    tileObject.gridTileLocation.HighlightTile(Color.green);
                } else {
                    //add poison icon above object
                    _statusIcon = addedTo.mapObjectVisual?.AddStatusIcon(this.name);
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if(_poisonedEffect != null) {
                ObjectPoolManager.Instance.DestroyObject(_poisonedEffect);
            }
            if (characterOwner != null) {
                characterOwner.AdjustDoNotRecoverHP(-1);
                ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);
            } else if (removedFrom is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.SetDefaultTileColor(Color.white);
                    tileObject.gridTileLocation.UnhighlightTile();
                } else {
                    if (_statusIcon != null) {
                        ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);    
                    }
                }
            }
            awareCharacters.Clear();
            responsibleCharacters?.Clear(); //Cleared list, for garbage collection
                                           //Messenger.Broadcast(Signals.OLD_NEWS_TRIGGER, sourceCharacter, gainedFromDoing);
        }
        public override string GetTestingData() {
            string summary = string.Empty;
            // WeightedDictionary<string> weights = GetResultWeights();
            // foreach (KeyValuePair<string, int> kvp in weights.dictionary) {
            //     summary += "(" + kvp.Key + "-" + kvp.Value.ToString() + ")";
            // }
            return summary;
        }
        public override void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) {
            base.ExecuteActionAfterEffects(action, goapNode, ref isRemoved);
            if (goapNode.action.actionCategory == ACTION_CATEGORY.CONSUME) {
                if(traitable is IPointOfInterest) {
                    IPointOfInterest poi = traitable as IPointOfInterest;
                    goapNode.actor.interruptComponent.TriggerInterrupt(INTERRUPT.Ingested_Poison, poi);
                    poi.traitContainer.RemoveTraitAndStacks(traitable, this);
                    isRemoved = true;
                }
            }
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            if(characterOwner != null) {
                characterOwner.AdjustHP(-Mathf.RoundToInt(characterOwner.maxHP * (0.5f * characterOwner.traitContainer.stacks[name])), ELEMENTAL_TYPE.Normal, true);
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

        // public WeightedDictionary<string> GetResultWeights() {
        //     WeightedDictionary<string> weights = new WeightedDictionary<string>();
        //     if (level == 1) {
        //         weights.AddElement("Sick", 80);
        //         weights.AddElement("Death", 20);
        //     } else if (level == 2) {
        //         weights.AddElement("Sick", 50);
        //         weights.AddElement("Death", 50);
        //     } else {
        //         weights.AddElement("Sick", 20);
        //         weights.AddElement("Death", 80);
        //     }
        //     return weights;
        // }
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
