using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class MusicLover : Trait {

        private Character owner;

        public MusicLover() {
            name = "Music Lover";
            description = "Music Lovers enjoy playing instruments and listening to music.";
            type = TRAIT_TYPE.SPECIAL;
            effect = TRAIT_EFFECT.POSITIVE;
            daysDuration = 0;
            mutuallyExclusive = new string[] { "Music Hater" };
        }

        #region Overrides
        public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
            if (targetPOI is Character) {
                Character seenCharacter = targetPOI as Character;
                if (seenCharacter.currentActionNode.action != null && seenCharacter.currentActionNode.action.isPerformingActualAction) {
                    if (seenCharacter.currentActionNode.action.goapType == INTERACTION_TYPE.PLAY_GUITAR) {
                        OnHearGuitarPlaying(seenCharacter);
                    } else if (seenCharacter.currentActionNode.action.goapType == INTERACTION_TYPE.SING) {
                        OnHearSinging(seenCharacter);
                    }
                }
            }
        }
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            owner = addedTo as Character;
            //if (owner.marker != null) {
            //Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
            //}
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            //Messenger.RemoveListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        }
        public override void OnDeath(Character character) {
            base.OnDeath(character);
            //Messenger.RemoveListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        }
        public override void OnReturnToLife(Character character) {
            base.OnReturnToLife(character);
            //Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        }
        public override void OnOwnerInitiallyPlaced(Character owner) {
            base.OnOwnerInitiallyPlaced(owner);
            //Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        }
        #endregion

        private void OnHearGuitarPlaying(Character guitarPlayer) {
            owner.traitContainer.AddTrait(owner, "Satisfied");
            owner.AdjustTiredness(20);
            owner.AdjustHappiness(40);
            //Debug.Log(GameManager.Instance.TodayLogString() + owner.name + " heard " + guitarPlayer.name + " playing a guitar, and became happier.");
            Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicLover", "heard_guitar");
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(guitarPlayer, guitarPlayer.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFrom(log, owner, guitarPlayer);
        }
        private void OnHearSinging(Character singer) {
            owner.traitContainer.AddTrait(owner, "Satisfied");
            owner.AdjustTiredness(20);
            owner.AdjustHappiness(40);
            //Debug.Log(GameManager.Instance.TodayLogString() + owner.name + " heard " + guitarPlayer.name + " playing a guitar, and became happier.");
            Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicLover", "heard_sing");
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(singer, singer.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotificationFrom(log, owner, singer);
        }

    }

}
