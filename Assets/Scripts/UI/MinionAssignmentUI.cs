using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionAssignmentUI : MonoBehaviour {
    public static MinionAssignmentUI Instance;

    public CharacterPortrait characterPortrait;

    private PlayerAbility _currentAbility;
    private Minion _currentAssignedMinion;

	void Awake () {
        Instance = this;
	}
	
	public void OpenUI() {
        characterPortrait.GeneratePortrait(null, 100, true);
    }
    public void SetCurrentAbility(PlayerAbility ability) {
        _currentAbility = ability;
    }
    public void ActivateAbility() {
        _currentAbility.Activate(PlayerAbilitiesUI.Instance.currentlySelectedInteractable, _currentAssignedMinion);
    }

    public void OnMinionDrop(Transform transform) {
        MinionItem minionItem = transform.GetComponent<MinionItem>();
        _currentAssignedMinion = minionItem.minion;
        characterPortrait.GeneratePortrait(minionItem.portrait.portraitSettings, 100, true);
    }
}
