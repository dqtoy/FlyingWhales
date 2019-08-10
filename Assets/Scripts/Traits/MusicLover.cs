using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicLover : Trait {

    private Character owner;

    public MusicLover() {
        name = "Music Lover";
        description = "This character loves music.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.POSITIVE;
        daysDuration = 0;
    }

    #region Overrides
    public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
        if (targetPOI is Character) {
            Character seenCharacter = targetPOI as Character;
            if (seenCharacter.currentAction != null && seenCharacter.currentAction.goapType == INTERACTION_TYPE.PLAY_GUITAR && seenCharacter.currentAction.isPerformingActualAction) {
                //The character will gain Cheery trait and will recover 20 Tiredness and 40 Happiness whenever it gets within vision of someone playing music. Should only trigger once upon entering vision.
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
        owner.AddTrait("Cheery");
        owner.AdjustTiredness(20);
        owner.AdjustHappiness(40);
        Debug.Log(GameManager.Instance.TodayLogString() + owner.name + " heard " + guitarPlayer.name + " playing a guitar, and became happier.");
        Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicLover", "heard_guitar");
        log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(guitarPlayer, guitarPlayer.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    private void OnActionStateSet(GoapAction action, GoapActionState state) {
        if (action.goapType == INTERACTION_TYPE.PLAY_GUITAR && owner.marker.inVisionCharacters.Contains(action.actor)) {
            OnHearGuitarPlaying(action.actor);
        }        
    }
}
