﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Wet : Trait {

        private StatusIcon _statusIcon;
        
        public Wet() {
            name = "Wet";
            description = "This is soaking wet.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(2); //if this trait is only temporary, then it should not advertise GET_WATER
            isStacking = true;
            moodEffect = -6;
            stackLimit = 10;
            stackModifier = 0f;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            addedTo.traitContainer.RemoveTrait(addedTo, "Burning");
            addedTo.traitContainer.RemoveTraitAndStacks(addedTo, "Overheating");
            if (addedTo is Character character) {
                character.needsComponent.AdjustComfortDecreaseRate(2f);
            }
            UpdateVisualsOnAdd(addedTo);
        }
        public override void OnStackTrait(ITraitable addedTo) {
            base.OnStackTrait(addedTo);
            UpdateVisualsOnAdd(addedTo);
        }
        public override void OnStackTraitAddedButStackIsAtLimit(ITraitable addedTo) {
            base.OnStackTraitAddedButStackIsAtLimit(addedTo);
            UpdateVisualsOnAdd(addedTo);
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.needsComponent.AdjustComfortDecreaseRate(-2f);
            }
            UpdateVisualsOnRemove(removedFrom);
        }
        #endregion

        private void UpdateVisualsOnAdd(ITraitable addedTo) {
            if (addedTo is Character character && _statusIcon == null) {
                _statusIcon = character.marker.AddStatusIcon(this.name);
            } else if (addedTo is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, 
                        InnerMapManager.Instance.assetManager.shoreTile, 0.5f);
                } else {
                    //add water icon above object
                    _statusIcon = addedTo.mapObjectVisual?.AddStatusIcon(this.name);
                }
            }
        }
        private void UpdateVisualsOnRemove(ITraitable removedFrom) {
            if (removedFrom is Character) {
                ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);
            } else if (removedFrom is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.parentMap.SetUpperGroundVisual(tileObject.gridTileLocation.localPlace, 
                        null);
                } else {
                    if (_statusIcon != null) {
                        ObjectPoolManager.Instance.DestroyObject(_statusIcon.gameObject);    
                    }
                }
            }
        }

    }
}
