using System.Collections;
using System.Collections.Generic;
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
                _statusIcon = character.marker.AddStatusIcon(this.name);
            } else if (addedTo is TileObject tileObject) {
                if (tileObject is GenericTileObject) {
                    tileObject.gridTileLocation.SetDefaultTileColor(Color.blue);
                    tileObject.gridTileLocation.HighlightTile(Color.blue);
                } else {
                  //add water icon above object
                  _statusIcon = addedTo.mapObjectVisual?.AddStatusIcon(this.name);
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.needsComponent.AdjustComfortDecreaseRate(-2f);
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
        }
        #endregion


    }
}
