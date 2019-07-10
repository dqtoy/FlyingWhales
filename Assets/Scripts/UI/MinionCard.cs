using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinionCard : MonoBehaviour {
    public Minion minion { get; private set; }

    public CharacterPortrait portrait;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtClass;
    public Image imgAbility1;
    public Image imgAbility2;
    public TextMeshProUGUI txtAbility1;
    public TextMeshProUGUI txtAbility2;
    public TextMeshProUGUI txtCombatAbility;
    public Image imgTrait1;
    public Image imgTrait2;
    public TextMeshProUGUI txtTrait1;
    public TextMeshProUGUI txtTrait2;

    public void SetMinion(Minion minion) {
        this.minion = minion;
        if(minion != null) {
            portrait.GeneratePortrait(minion.character);
            txtName.text = minion.character.name;
            txtClass.text = minion.character.raceClassName;

            PlayerJobAction ability1 = minion.interventionAbilities[0];
            imgAbility1.sprite = PlayerManager.Instance.GetJobActionSprite(ability1.name);
            txtAbility1.text = ability1.name;

            PlayerJobAction ability2 = minion.interventionAbilities[1];
            imgAbility2.sprite = PlayerManager.Instance.GetJobActionSprite(ability2.name);
            txtAbility2.text = ability2.name;

            txtCombatAbility.text = minion.combatAbility.name;

            //TODO: trait1 and trait2
        }
    }
}
