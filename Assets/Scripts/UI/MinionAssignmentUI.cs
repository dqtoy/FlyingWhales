using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinionAssignmentUI : MonoBehaviour {
    public static MinionAssignmentUI Instance;

    public CharacterPortrait characterPortrait;
    public GameObject minionAssignmentGO;
    public Button activateBtn;

    private PlayerAbility _currentAbility;
    private Minion _currentAssignedMinion;

	void Awake () {
        Instance = this;
	}
	
	public void OpenUI() {
        characterPortrait.GeneratePortrait(null, 100, true);
        minionAssignmentGO.SetActive(true);
        activateBtn.interactable = false;
    }
    public void CloseUI() {
        minionAssignmentGO.SetActive(false);
    }
    public void SetCurrentAbility(PlayerAbility ability) {
        _currentAbility = ability;
    }
    public void ActivateAbility() {
        _currentAbility.Activate(PlayerAbilitiesUI.Instance.currentlySelectedInteractable, _currentAssignedMinion);
        activateBtn.interactable = false;
    }

    public void OnMinionDrop(Transform transform) {
        MinionItem minionItem = transform.GetComponent<MinionItem>();
        _currentAssignedMinion = minionItem.minion;
        characterPortrait.GeneratePortrait(minionItem.portrait.portraitSettings, 100, true);
        if(_currentAssignedMinion != null) {
            activateBtn.interactable = true;
        }
    }
}
