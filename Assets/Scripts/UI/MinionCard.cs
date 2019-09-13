using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MinionCard : MonoBehaviour {
    public Minion minion { get; private set; }

    public Image portraitImg;
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
    public TextMeshProUGUI txtActions;
    public TextMeshProUGUI txtResearch;
    public GameObject researchGO;

    //Minion Data
    public string minionName { get; private set; }
    public string className { get; private set; }
    public COMBAT_ABILITY combatAbilityType { get; private set; }
    public List<INTERVENTION_ABILITY> abilitiesToResearch { get; private set; }
    //TODO: trait 1 and 2

    public void SetMinion(string className, COMBAT_ABILITY combatAbility, string minionName = "") {
        if(minionName == string.Empty) {
            this.minionName = RandomNameGenerator.Instance.GenerateMinionName();
        } else {
            this.minionName = minionName;
        }
        combatAbilityType = combatAbility;
        this.className = className;

        Sprite classPortrait = CharacterManager.Instance.GetClassPortraitSprite(className);
        if(classPortrait != null) {
            portraitImg.sprite = classPortrait;
            portraitImg.gameObject.SetActive(true);
        } else {
            portraitImg.gameObject.SetActive(false);
        }
        txtName.text = minionName;
        txtClass.text = "Demon " + className;
        txtCombatAbility.text = Utilities.NormalizeStringUpperCaseFirstLetters(combatAbilityType.ToString());

        DeadlySin deadlySin = CharacterManager.Instance.GetDeadlySin(className);
        string actions = string.Empty;
        for (int i = 0; i < deadlySin.assignments.Length; i++) {
            if(i > 0) {
                actions += ", ";
            }
            actions += Utilities.NormalizeStringUpperCaseFirstLetters(deadlySin.assignments[i].ToString());
        }
        txtActions.text = actions;

        abilitiesToResearch = CharacterManager.Instance.Get3RandomResearchInterventionAbilities(deadlySin);
        if (abilitiesToResearch.Count > 0) {
            string research = string.Empty;
            for (int i = 0; i < abilitiesToResearch.Count; i++) {
                if (i > 0) {
                    research += ", ";
                }
                research += Utilities.NormalizeStringUpperCaseFirstLetters(abilitiesToResearch[i].ToString());
            }
            txtResearch.text = research;
            researchGO.SetActive(true);
        } else {
            researchGO.SetActive(false);
        }
    }

    public void SetMinion(Minion minion) {
        this.minion = minion;
        if(minion != null) {
            //portrait.GeneratePortrait(minion.character);
            txtName.text = minion.character.name;
            txtClass.text = minion.character.raceClassName;

            txtCombatAbility.text = minion.combatAbility.name;

            Sprite classPortrait = CharacterManager.Instance.GetClassPortraitSprite(className);
            if (classPortrait != null) {
                portraitImg.sprite = classPortrait;
                portraitImg.gameObject.SetActive(true);
            } else {
                portraitImg.gameObject.SetActive(false);
            }
            //TODO: trait1 and trait2

            string actions = string.Empty;
            for (int i = 0; i < minion.deadlySin.assignments.Length; i++) {
                if (i > 0) {
                    actions += ", ";
                }
                actions += Utilities.NormalizeStringUpperCaseFirstLetters(minion.deadlySin.assignments[i].ToString());
            }
            txtActions.text = actions;

            if(minion.interventionAbilitiesToResearch.Count > 0) {
                string research = string.Empty;
                for (int i = 0; i < minion.interventionAbilitiesToResearch.Count; i++) {
                    if (i > 0) {
                        research += ", ";
                    }
                    research += Utilities.NormalizeStringUpperCaseFirstLetters(minion.interventionAbilitiesToResearch[i].ToString());
                }
                txtResearch.text = research;
                researchGO.SetActive(true);
            } else {
                researchGO.SetActive(false);
            }
        }
    }

    //public void OnHoverInterventionAbility1() {
    //    UIManager.Instance.ShowSmallInfo(minion.interventionAbilities[0].dynamicDescription);
    //}
    public void OnExitHoverInterventionAbility1() {
        UIManager.Instance.HideSmallInfo();
    }
    //public void OnHoverInterventionAbility2() {
    //    UIManager.Instance.ShowSmallInfo(minion.interventionAbilities[1].dynamicDescription);
    //}
    public void OnExitHoverInterventionAbility2() {
        UIManager.Instance.HideSmallInfo();
    }
    //public void OnHoverCombatAbility() {
    //    UIManager.Instance.ShowSmallInfo(minion.combatAbility.dynamicDescription);
    //}
    //public void OnExitHoverCombatAbility() {
    //    UIManager.Instance.HideSmallInfo();
    //}
}
