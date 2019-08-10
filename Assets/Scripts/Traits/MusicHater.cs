using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicHater : Trait {

    private Character owner;

    public MusicHater() {
        name = "Music Hater";
        description = "This character hates music.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
    }

    #region Overrides
    public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
        if (targetPOI is Character) {
            Character seenCharacter = targetPOI as Character;
            if (seenCharacter.currentAction != null && seenCharacter.currentAction.goapType == INTERACTION_TYPE.PLAY_GUITAR && seenCharacter.currentAction.isPerformingActualAction) {
                OnHearGuitarPlaying(seenCharacter);
            }
        }
    }
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        owner = addedTo as Character;
        if (owner.marker != null) {
            Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
        }
    }
    public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
        base.OnRemoveTrait(removedFrom, removedBy);
        Messenger.RemoveListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
    }
    public override void OnOwnerInitiallyPlaced(Character owner) {
        base.OnOwnerInitiallyPlaced(owner);
        Messenger.AddListener<GoapAction, GoapActionState>(Signals.ACTION_STATE_SET, OnActionStateSet);
    }
    #endregion

    private void OnHearGuitarPlaying(Character guitarPlayer) {
        //The character will gain Annoyed trait whenever it gets within vision of someone playing music. If Actor has a Lover or Paramour relationship with the target, create a Break Up job for the Actor:
        owner.AddTrait("Annoyed");
        if (owner.HasRelationshipOfTypeWith(guitarPlayer, false, RELATIONSHIP_TRAIT.LOVER, RELATIONSHIP_TRAIT.PARAMOUR)) {
            if (owner.CreateBreakupJob(guitarPlayer) != null) {
                Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "break_up");
                log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(guitarPlayer, guitarPlayer.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                log.AddLogToInvolvedObjects();
                PlayerManager.Instance.player.ShowNotification(log);
            }
        } else if (!owner.HasRelationshipOfTypeWith(guitarPlayer, RELATIONSHIP_TRAIT.ENEMY)) {
            //Otherwise, if the Actor does not yet consider the Target an Enemy, relationship degradation will occur, log:
            Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "degradation");
            log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(guitarPlayer, guitarPlayer.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
            CharacterManager.Instance.RelationshipDegradation(guitarPlayer, owner);
        }
        Debug.Log(GameManager.Instance.TodayLogString() + owner.name + " heard " + guitarPlayer.name + " playing a guitar, and became annoyed.");
        
    }
    private void OnActionStateSet(GoapAction action, GoapActionState state) {
        if (action.goapType == INTERACTION_TYPE.PLAY_GUITAR && owner.marker.inVisionCharacters.Contains(action.actor)) {
            OnHearGuitarPlaying(action.actor);
        }
    }
}
