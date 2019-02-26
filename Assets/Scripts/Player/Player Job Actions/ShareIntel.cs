using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShareIntel : PlayerJobAction {

    public Character targetCharacter { get; private set; }

    public ShareIntel() {
        actionName = "Share Intel";
        SetDefaultCooldownTime(10);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        //base.ActivateAction(assignedCharacter, targetCharacter);
        SetSubText("Pick intel to share with " +  targetCharacter.name);
        UIManager.Instance.OpenShareIntelMenu(targetCharacter, assignedCharacter);
        
        //PlayerUI.Instance.SetIntelMenuState(true);
        //PlayerUI.Instance.SetIntelItemClickActions(targetCharacter.ShareIntel);
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => base.ActivateAction(assignedCharacter, targetCharacter));
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => SetTargetCharacter(targetCharacter));
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => SetSubText(string.Empty));
        //PlayerUI.Instance.AddIntelItemOtherClickActions(() => PlayerUI.Instance.SetIntelItemClickActions(null));
    }
    public void BaseActivate(Character targetCharacter) {
        base.ActivateAction(assignedCharacter, targetCharacter);
        SetSubText(string.Empty);
        SetTargetCharacter(targetCharacter);
    }
    public override void DeactivateAction() {
        base.DeactivateAction();
        targetCharacter = null;
        SetSubText(string.Empty);
    }
    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (character.id == targetCharacter.id) {
            return false;
        }
        if (this.targetCharacter != null && targetCharacter.id == this.targetCharacter.id) {
            return false;
        }
        if (PlayerManager.Instance.player.allIntel.Count == 0) {
            return false;
        }
        if (UIManager.Instance.IsShareIntelMenuOpen()) {
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }
    protected override void OnCharacterDied(Character characterThatDied) {
        base.OnCharacterDied(characterThatDied);
        if (!this.isActive) {
            return; //if this action is no longer active, do not check if the character that died was the target
        }
        if (characterThatDied.id == targetCharacter.id) {
            DeactivateAction();
        }
    }
    private void SetTargetCharacter(Character targetCharacter) {
        this.targetCharacter = targetCharacter;
    }

    public List<string> ReactToIntel(InteractionIntel intel) {
        List<string> reactions = new List<string>();
        if (Mathf.Abs(GameManager.Instance.continuousDays - intel.actionDeadline) >= 72) {
            //Intel's day tick is already over 72 ticks old
            reactions.Add("Why are you telling me this only now? It is already too late.");
        } else if (targetCharacter.GetCharacterRelationshipData(intel.actor) == null
            && (intel.targetCharacter == null || targetCharacter.GetCharacterRelationshipData(intel.targetCharacter) == null)) {
            //Intel's Actor and Target do not have any relationship with character
            reactions.Add("How is this relevant to me?");
        } else {
            CharacterRelationshipData relDataActor = targetCharacter.GetCharacterRelationshipData(intel.actor);
            CharacterRelationshipData relDataTarget = null;
            CharacterRelationshipData relDataBetween = null;
            if (intel.targetCharacter != null) {
                relDataTarget = targetCharacter.GetCharacterRelationshipData(intel.targetCharacter);
                relDataBetween = intel.actor.GetCharacterRelationshipData(targetCharacter);
            }
            /*1. Actor is a Lover of character, and the Action Type is Romantic*/
            if (relDataActor != null && relDataActor.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER) && intel.IsOfCategory(INTERACTION_CATEGORY.ROMANTIC)) {
                /*- Target will become Enemy of character, set **known character location structure** as Intel structure
                  - Character reaction: "I don't believe this! How could [Actor Name] do this to me?"*/
                relDataTarget = CharacterManager.Instance.CreateNewRelationshipBetween(targetCharacter, intel.targetCharacter, RELATIONSHIP_TRAIT.ENEMY);
                relDataTarget.SetKnownStructure(intel.actionLocationStructure);
                reactions.Add("I don't believe this! How could " + intel.actor.name + " do this to me?");
            }
            /*2. Target is a Lover of character, and the Action Type is Romantic*/
             else if (relDataTarget != null && relDataTarget.HasRelationshipTrait(RELATIONSHIP_TRAIT.LOVER) && intel.IsOfCategory(INTERACTION_CATEGORY.ROMANTIC)) {
                /*- Actor will become Enemy of character, set **known character location structure** as Intel structure
                  - Character reaction: "I don't believe this! How could [Actor Name] do this to me?"*/
                relDataActor = CharacterManager.Instance.CreateNewRelationshipBetween(targetCharacter, intel.actor, RELATIONSHIP_TRAIT.ENEMY);
                relDataActor.SetKnownStructure(intel.actionLocationStructure);
                reactions.Add("I don't believe this! How could " + intel.targetCharacter.name + " do this to me?");
            }
            /*3. Actor has a positive relationship with the character and character's **character missing** data with the Actor is True
             - **character located** will be set to True
             - **known character location structure** will be set to Intel structure
             - if Intel Actor Effect includes any negative Disabler or Status traits, add them to **character trouble**
             - Character reaction: "Thank you for letting me now [Actor Name]'s status. I should do something about this."*/
             else if (relDataActor != null && relDataActor.HasRelationshipOfEffect(TRAIT_EFFECT.POSITIVE) && relDataActor.isCharacterMissing) {
                relDataActor.SetIsCharacterLocated(true);
                relDataActor.SetKnownStructure(intel.actionLocationStructure);
                if (intel.HasActorEffect(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                    relDataActor.AddTrouble(intel.actor.GetTrait(intel.GetActorEffect(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER).effectString));
                } else if (intel.HasActorEffect(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.STATUS)) {
                    relDataActor.AddTrouble(intel.actor.GetTrait(intel.GetActorEffect(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.STATUS).effectString));
                }
                reactions.Add("Thank you for letting me now " + intel.actor.name + "'s status. I should do something about this.");
            }
            /*4. Target has a positive relationship with the character and character's **character missing** data with the Target is True
             - **character located** will be set to True
             - **known character location structure** will be set to Intel structure
             - if Intel Target Effect includes any negative Disabler or Status traits, add them to **character trouble**
             - Character reaction: "Thank you for letting me now [Target Name]'s status. I should do something about this."*/
             else if (relDataTarget != null && relDataTarget.HasRelationshipOfEffect(TRAIT_EFFECT.POSITIVE) && relDataTarget.isCharacterMissing) {
                relDataTarget.SetIsCharacterLocated(true);
                relDataTarget.SetKnownStructure(intel.actionLocationStructure);
                if (intel.HasTargetEffect(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                    relDataActor.AddTrouble(intel.actor.GetTrait(intel.GetTargetEffect(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER).effectString));
                } else if (intel.HasTargetEffect(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.STATUS)) {
                    relDataActor.AddTrouble(intel.actor.GetTrait(intel.GetTargetEffect(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.STATUS).effectString));
                }
                reactions.Add("Thank you for letting me now " + intel.targetCharacter.name + "'s status. I should do something about this.");
            }

            /*5. Actor is an enemy of the character, The Target character is not a Relative of Actor and The action type is unknown.
             - 50 Weight that the character will think that the Target is important to the Actor
               - if not yet in there, add Target to **known loved ones** of Actor
               - Character reaction: "I wonder what [Actor Name] is doing with [Target Name]? There must be a special relationship between them."
             - 50 Weight that the character will not jump to any conclusions
               - Character reaction: "I wonder what [Actor Name] is doing with [Target Name]? I need to find out more before jumping to conclusions."*/
            else if (relDataActor != null && relDataActor.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY)
            && relDataBetween != null && relDataBetween.HasRelationshipTrait(RELATIONSHIP_TRAIT.RELATIVE)
            && intel.categories == null) {
                WeightedDictionary<string> result = new WeightedDictionary<string>();
                result.AddElement("Assume", 50);
                result.AddElement("Don't Assume", 50);
                switch (result.PickRandomElementGivenWeights()) {
                    case "Assume":
                        relDataActor.AddKnownLovedOne(intel.targetCharacter);
                        reactions.Add("I wonder what " + intel.actor.name + " is doing with " + intel.targetCharacter.name + "? There must be a special relationship between them.");
                        break;
                    case "Don't Assume":
                        reactions.Add("I wonder what " + intel.actor.name + " is doing with " + intel.targetCharacter.name + "? I need to find out more before jumping to conclusions.");
                        break;
                }
            }
            /*6. Target is an enemy of the character. The Actor is not a Relative of Target. The action type is unknown.
             - 50 Weight that the character will think that the Actor is important to the Target
               - if not yet in there, add Actor to **known loved ones** of Target
               - Character reaction: "I wonder what [Target Name] is doing with [Actor Name]? There must be a special relationship between them."
             - 50 Weight that the character will not jump to any conclusions
               - Character reaction: "I wonder what [Target Name] is doing with [Actor Name]? I need to find out more before jumping to conclusions."*/
            else if (relDataTarget != null && relDataTarget.HasRelationshipTrait(RELATIONSHIP_TRAIT.ENEMY)
            && relDataBetween != null && relDataBetween.HasRelationshipTrait(RELATIONSHIP_TRAIT.RELATIVE)
            && intel.categories == null) {
                WeightedDictionary<string> result = new WeightedDictionary<string>();
                result.AddElement("Assume", 50);
                result.AddElement("Don't Assume", 50);
                switch (result.PickRandomElementGivenWeights()) {
                    case "Assume":
                        relDataTarget.AddKnownLovedOne(intel.actor);
                        reactions.Add("I wonder what " + intel.targetCharacter.name + " is doing with " + intel.actor.name + "? There must be a special relationship between them.");
                        break;
                    case "Don't Assume":
                        reactions.Add("I wonder what " + intel.targetCharacter.name + " is doing with " + intel.actor.name + "? I need to find out more before jumping to conclusions.");
                        break;
                }
            }
        }
        return reactions;
    }

}
