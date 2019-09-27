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
    public UnsummonedMinionData minionData { get; private set; }
    //public List<INTERVENTION_ABILITY> abilitiesToResearch { get; private set; }
    //TODO: trait 1 and 2

    public void SetMinion(UnsummonedMinionData minionData) {
        this.minionData = minionData;
        string minionName = this.minionData.minionName;
        if (minionName == string.Empty) {
            UnsummonedMinionData temp = this.minionData;
            temp.minionName = RandomNameGenerator.Instance.GenerateMinionName();
            this.minionData = temp;
        }

        Sprite classPortrait = CharacterManager.Instance.GetClassPortraitSprite(this.minionData.className);
        if(classPortrait != null) {
            portraitImg.sprite = classPortrait;
            portraitImg.gameObject.SetActive(true);
        } else {
            portraitImg.gameObject.SetActive(false);
        }
        txtName.text = minionName;
        txtClass.text = "Demon " + this.minionData.className;
        txtCombatAbility.text = "<link=\"0\">" + Utilities.NormalizeStringUpperCaseFirstLetters(this.minionData.combatAbility.ToString()) + "</link>";

        DeadlySin deadlySin = CharacterManager.Instance.GetDeadlySin(this.minionData.className);
        string actions = string.Empty;
        for (int i = 0; i < deadlySin.assignments.Length; i++) {
            if(i > 0) {
                actions += ", ";
            }
            actions += "<link=\"" + i.ToString() + "\">" + Utilities.NormalizeStringUpperCaseFirstLetters(deadlySin.assignments[i].ToString()) + "</link>";
        }
        txtActions.text = actions;

        if (this.minionData.interventionAbilitiesToResearch.Count > 0) {
            string research = string.Empty;
            for (int i = 0; i < this.minionData.interventionAbilitiesToResearch.Count; i++) {
                if (i > 0) {
                    research += ", ";
                }
                research += "<link=\"" + i.ToString() + "\">" + Utilities.NormalizeStringUpperCaseFirstLetters(this.minionData.interventionAbilitiesToResearch[i].ToString()) + "</link>";
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

            txtCombatAbility.text = "<link=\"0\">" + minion.combatAbility.name + "</link>";

            Sprite classPortrait = CharacterManager.Instance.GetClassPortraitSprite(minion.character.characterClass.className);
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
                actions += "<link=\"" + i.ToString() + "\">" + Utilities.NormalizeStringUpperCaseFirstLetters(minion.deadlySin.assignments[i].ToString()) + "</link>";
            }
            txtActions.text = actions;

            if(minion.interventionAbilitiesToResearch.Count > 0) {
                string research = string.Empty;
                for (int i = 0; i < minion.interventionAbilitiesToResearch.Count; i++) {
                    if (i > 0) {
                        research += ", ";
                    }
                    research += "<link=\"" + i.ToString() + "\">" + Utilities.NormalizeStringUpperCaseFirstLetters(minion.interventionAbilitiesToResearch[i].ToString()) + "</link>";
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

    public void OnHoverActionAbility(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            DEADLY_SIN_ACTION action;
            if (minion != null) {
                action = minion.deadlySin.assignments[index];
            } else {
                DeadlySin deadlySin = CharacterManager.Instance.GetDeadlySin(this.minionData.className);
                action = deadlySin.assignments[index];
            }
            UIManager.Instance.ShowSmallInfo(action.Description(), Utilities.NormalizeStringUpperCaseFirstLetters(action.ToString()));
        }
    }
    public void OnHoverExitAbility() {
        UIManager.Instance.HideSmallInfo();
    }

    public void OnHoverResearchSpell(object obj) {
        if (obj is string) {
            int index = System.Int32.Parse((string)obj);
            INTERVENTION_ABILITY spell;
            if (minion != null) {
                spell = minion.interventionAbilitiesToResearch[index];
            } else {
                spell = minionData.interventionAbilitiesToResearch[index];
            }
            UIManager.Instance.ShowSmallInfo(PlayerManager.Instance.allInterventionAbilitiesData[spell].description, Utilities.NormalizeStringUpperCaseFirstLetters(spell.ToString()));
        }
    }
    public void OnHoverExitSpell() {
        UIManager.Instance.HideSmallInfo();
    }


    public void OnHoverCombatAbility(object obj) {
        COMBAT_ABILITY action;
        if (minion != null) {
            action = minion.combatAbility.type;
        } else {
            action = minionData.combatAbility;
        }
        UIManager.Instance.ShowSmallInfo(action.Description(), Utilities.NormalizeStringUpperCaseFirstLetters(action.ToString()));
    }
    public void OnHoverExitCombatAbility() {
        UIManager.Instance.HideSmallInfo();
    }
}
