using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.UI.Extensions;
using System.Linq;

public class TheProfaneUI : MonoBehaviour {

    [Header("General")]
    [SerializeField] private HorizontalScrollSnap scrollSnap;
    [SerializeField] private Button nextBtn;
    [SerializeField] private TextMeshProUGUI nextBtnLbl;

    [Header("Minion")]
    [SerializeField] private ScrollRect minionsScrollRect;
    [SerializeField] private GameObject minionItemPrefab;
    [SerializeField] private ToggleGroup minionsToggleGroup;

    [Header("Cultist")]
    [SerializeField] private ScrollRect charactersScrollRect;
    [SerializeField] private GameObject characterItemPrefab;
    [SerializeField] private ToggleGroup cultistsToggleGroup;

    [Header("Actions")]
    [SerializeField] private ScrollRect actionsScrollRect;
    [SerializeField] private GameObject stringItemPrefab;

    public TheProfane profane { get; private set; }
    public Minion chosenMinion { get; private set; }
    public Character chosenCharacter { get; private set; }
    public string chosenAction { get; private set; }

    private enum Profane_Step { Minion, Character, Action }
    private Profane_Step currentStep;

    #region General
    public void ShowTheProfaneUI(TheProfane profane) {
        this.profane = profane;
        //UpdateMinionList();
        gameObject.SetActive(true);
        Reset();
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
    public void UpdatePlayerDelayDivineInterventionUI() { }
    public void SetCurrentStep(int page) {
        currentStep = (Profane_Step)page;
        //update next and previous button;
        UpdateNextButton(true);
    }
    private void UpdateNextButton(bool updateList = false) {
        if (currentStep == Profane_Step.Minion) {
            nextBtn.interactable = chosenMinion != null;
            nextBtnLbl.text = "Next";
            if (updateList) {
                UpdateMinionList();
            }
        } else if (currentStep == Profane_Step.Character) {
            nextBtn.interactable = chosenCharacter != null;
            nextBtnLbl.text = "Next";
            if (updateList) {
                UpdateCharacterList();
            }
        } else if (currentStep == Profane_Step.Action) {
            if (updateList) {
                UpdatePossibleActions();
            }
        }
        Debug.Log("Set next btn interactable to " + nextBtn.interactable.ToString());
    }
    private void Reset() {
        chosenMinion = null;
        //scrollSnap.GoToScreen(0);
        currentStep = Profane_Step.Minion;
        UpdateNextButton(true);
    }
    #endregion

    #region Minion
    private void UpdateMinionList() {
        Utilities.DestroyChildren(minionsScrollRect.content);
        List<Minion> minions = new List<Minion>(PlayerManager.Instance.player.minions);
        for (int i = 0; i < minions.Count; i++) {
            Minion currMinion = minions[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(minionItemPrefab.name, Vector3.zero, Quaternion.identity, minionsScrollRect.content);
            MinionCharacterItem item = go.GetComponent<MinionCharacterItem>();
            item.SetCharacter(currMinion.character);
            if (currMinion.assignedRegion == null) {
                //can be assigned
                item.SetAsToggle(minionsToggleGroup);
                item.AddOnToggleAction(() => OnClickMinion(currMinion), true);
                if (chosenMinion == currMinion) {
                    item.SetToggleState(true);
                    SetSelectedMinion(currMinion);
                } else if (chosenMinion == null) {
                    item.SetToggleState(true);
                    SetSelectedMinion(currMinion);
                }
                item.SetCoverState(false);
            } else {
                //cannot be assigned
                item.ResetToggle();
                item.ClearClickActions();
                if (chosenMinion == currMinion) {
                    SetSelectedMinion(null);
                    item.SetToggleState(false);
                }
                go.transform.SetAsLastSibling();
                //cannot be assigned
                item.SetCoverState(true);
            }
        }
    }
    private void OnClickMinion(Minion minion) {
        if (chosenMinion == minion) {
            SetSelectedMinion(null);
        } else {
            SetSelectedMinion(minion);
        }
    }
    private void SetSelectedMinion(Minion minion) {
        chosenMinion = minion;
        Debug.Log("Selected Minion is " + (chosenMinion?.character.name ?? "Null"));
        //update next button
        UpdateNextButton();
    }
    #endregion

    #region Cultist
    private void UpdateCharacterList() {
        Utilities.DestroyChildren(charactersScrollRect.content);
        List<Character> allCharacters = new List<Character>(CharacterManager.Instance.allCharacters.Where(x => !x.returnedToLife && !x.isDead && x.role.roleType != CHARACTER_ROLE.MINION && x.GetNormalTrait("Disillusioned", "Evil", "Treacherous") != null && x.GetNormalTrait("Blessed") == null));
        for (int i = 0; i < allCharacters.Count; i++) {
            Character currCharacter = allCharacters[i];
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterItemPrefab.name, Vector3.zero, Quaternion.identity, charactersScrollRect.content);
            CharacterItem item = go.GetComponent<CharacterItem>();
            item.SetCharacter(currCharacter);
            item.ClearClickActions();
            item.SetCoverState(CanConvertCharacterToCultist(currCharacter));
            if (item.coverState) {
                item.ResetToggle();
                go.transform.SetAsLastSibling();
                if (chosenCharacter == currCharacter) {
                    SetSelectedCharacter(null);
                    item.SetToggleState(false);
                }
            } else {
                item.SetAsToggle(minionsToggleGroup);
                item.AddOnToggleAction(() => OnClickCharacter(currCharacter), true);
                if (chosenCharacter == currCharacter) {
                    SetSelectedCharacter(currCharacter);
                    item.SetToggleState(true);
                }
            }
           
        }
    }
    private void OnClickCharacter(Character character) {
        if (chosenCharacter == character) {
            SetSelectedCharacter(null);
        } else {
            SetSelectedCharacter(character);
        }
    }
    private void SetSelectedCharacter(Character character) {
        chosenCharacter = character;
        Debug.Log("Selected Character is " + (chosenCharacter?.name ?? "Null"));
        //update next button
        UpdateNextButton();
    }
    private bool CanConvertCharacterToCultist(Character character) {
        int manaCost = profane.GetConvertToCultistCost(character);
        if (PlayerManager.Instance.player.mana < manaCost) {
            return false;
        }
        if (character.GetNormalTrait("Evil") != null) {
            return true;
        } else if (character.GetNormalTrait("Disillusioned") != null) {
            return true;
        } else if (character.GetNormalTrait("Treacherous") != null) {
            Character factionLeader = character.faction.leader as Character;
            return character.HasRelationshipOfTypeWith(factionLeader, RELATIONSHIP_TRAIT.ENEMY);
        }
        return false;
    }
    #endregion

    #region Actions
    private void UpdatePossibleActions() {
        Utilities.DestroyChildren(actionsScrollRect.content);
        if (chosenCharacter.GetNormalTrait("Cultist") == null) {
            //character is not yet a cultist
            //only action available is convert to cultist
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(stringItemPrefab.name, Vector3.zero, Quaternion.identity, actionsScrollRect.content);
            StringPickerItem item = go.GetComponent<StringPickerItem>();
            item.SetString("Convert to cultist", string.Empty);
            item.onClickAction = SetChosenAction;
        } else {
            //character is a cultist
            List<string> actions = GetPossibleActionsForCharacter(chosenCharacter);
            for (int i = 0; i < actions.Count; i++) {
                GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(stringItemPrefab.name, Vector3.zero, Quaternion.identity, actionsScrollRect.content);
                StringPickerItem item = go.GetComponent<StringPickerItem>();
                item.SetString(actions[i], string.Empty);
                item.onClickAction = SetChosenAction;
            }
        }
    }
    private void SetChosenAction(string action) {
        chosenAction = action;
        //show confirmation.
        UIManager.Instance.ShowYesNoConfirmation("Confirm Action", "Are you sure you want to " + chosenAction + " " + chosenCharacter.name + "?", onClickYesAction: OnConfirmAction, showCover: true, layer: 25);

    }
    private void OnConfirmAction() {
        profane.DoAction(chosenCharacter, chosenAction);
    }
    private List<string> GetPossibleActionsForCharacter(Character character) {
        List<string> actions = new List<string>();
        if (character.role.roleType != CHARACTER_ROLE.MINION) {
            actions.Add("Corrupt");
        } else if (character.homeRegion.area != null && character.homeRegion.IsFactionHere(character.faction)) {
            actions.Add("Sabotage Faction Quest");
        } else if (character.homeRegion.area != null) {
            actions.Add("Destroy Supply");
            actions.Add("Destroy Food");
        }
        return actions;
    }
    #endregion
}
