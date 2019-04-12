using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoobyTrapped : StructureTrait {

    private WeightedDictionary<string> trapWeights;

	public BoobyTrapped(LocationStructure owner) : base(owner) {
        name = "Booby Trapped";
        /*
         Anyone that enters the structure will trigger a trap check.
            Trap Kills: Weight +50
            Trap Injures: Weight +20
            Trap Escaped: Weight +10
         */
        trapWeights = new WeightedDictionary<string>();
        trapWeights.AddElement("Trap Kills", 50);
        trapWeights.AddElement("Trap Injures", 20);
        trapWeights.AddElement("Trap Escaped", 10);
    }

    #region Overrides
    public override void OnCharacterEnteredStructure(Character character) {
        string result = trapWeights.PickRandomElementGivenWeights();
        switch (result) {
            case "Trap Kills":
                TrapKills(character);
                break;
            case "Trap Injures":
                TrapInjures(character);
                break;
            case "Trap Escaped":
                TrapEscaped(character);
                break;
            default:
                break;
        }
        owner.RemoveTrait(this);
    }
    #endregion

    private void TrapKills(Character character) {
        /*The character dies and will cancel any active interaction from proceeding.
        Log: "[Character Name] activated the booby trap and was immediately killed!"*/
        character.Death();
        Log log = new Log(GameManager.Instance.Today(), "StructureTraits", this.GetType().ToString(), "Trap Kills");
        log.AddToFillers(new LogFiller(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
        character.AddHistory(log);
        owner.location.AddHistory(log);
    }
    private void TrapInjures(Character character) {
        /*The character gains Injured trait and his active interaction will continue.
        Log: "[Character Name] activated the booby trap and was injured!"*/
        Injured injured = new Injured();
        character.AddTrait(injured);
        Log log = new Log(GameManager.Instance.Today(), "StructureTraits", this.GetType().ToString(), "Trap Injures");
        log.AddToFillers(new LogFiller(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
        character.AddHistory(log);
        owner.location.AddHistory(log);
    }
    private void TrapEscaped(Character character) {
        /*The character continues with his active interaction.
        Log: "[Character Name] activated the booby trap but managed to evade harm completely!"*/
        Log log = new Log(GameManager.Instance.Today(), "StructureTraits", this.GetType().ToString(), "Trap Escaped");
        log.AddToFillers(new LogFiller(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER));
        character.AddHistory(log);
        owner.location.AddHistory(log);
    }
}
