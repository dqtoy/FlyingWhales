using UnityEngine;
using System.Collections;

public class CharacterSummaryEntry : MonoBehaviour {

    private ECS.Character _character;

    [SerializeField] private UI2DSprite bgSprite;
    [SerializeField] private UILabel characterNameLbl;
    [SerializeField] private UILabel factionNameLbl;
    [SerializeField] private UILabel raceLbl;
    [SerializeField] private UILabel roleLbl;

    #region getters/setters
    public ECS.Character character {
        get { return _character; }
    }
    #endregion

    public void SetCharacter(ECS.Character character) {
        _character = character;
        UpdateCharacterInfo();
    }

    public void UpdateCharacterInfo() {
        characterNameLbl.text = character.name;
        if (_character.isFactionless) {
            factionNameLbl.text = "Unaligned";
        } else {
            factionNameLbl.text = character.faction.name;
            bgSprite.color = character.faction.factionColor;
        }
        raceLbl.text = Utilities.NormalizeString(character.raceSetting.race.ToString());
        roleLbl.text = Utilities.NormalizeString(character.role.roleType.ToString());
    }

    private void OnClick() {
        UIManager.Instance.ShowCharacterInfo(_character);
    }

    #region Sorting
    public void OnOrderByName() {
        this.name = _character.name;
    }
    public void OnOrderByFaction() {
        if (_character.faction == null) {
            this.name = "Unaligned";
        } else {
            this.name = _character.faction.name;
        }
    }
    public void OnOrderByRace() {
        this.name = _character.raceSetting.race.ToString();
    }
    public void OnOrderByRole() {
        this.name = _character.role.roleType.ToString();
    }
    #endregion

}
