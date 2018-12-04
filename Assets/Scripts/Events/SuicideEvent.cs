
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideEvent : GameEvent {

    private Character character;

    public SuicideEvent() : base(GAME_EVENT.SUICIDE) {
    }

    public override void Initialize(List<Character> characters) {
        base.Initialize(characters);
        this.character = characters[0];

        //schedule suicide action
        CharacterAction suicideAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.SUICIDE);
        eventActions[character].Enqueue(new EventAction(suicideAction, character.homeLandmark.landmarkObj, this, 1));

        character.AddScheduledEvent(this);
    }

    public override bool MeetsRequirements(Character character) {
        //Character generalMax = CharacterManager.Instance.GetCharacterByClass("General");
        //if (!character.HasRelationshipWith(generalMax)) {
        //    return false; //Lady of the lake does not have relationship with general max
        //}
        //if (!character.HasRelationshipStatusWith(generalMax, CHARACTER_RELATIONSHIP.LOVER)) {
        //    return false; //Lady of the lake and general max are not lovers
        //}
        return base.MeetsRequirements(character);
        
    }


}
