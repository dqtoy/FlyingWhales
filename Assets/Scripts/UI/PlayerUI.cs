using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using TMPro;

public class PlayerUI : MonoBehaviour {
    public static PlayerUI Instance;

    public TextMeshProUGUI manaText;
    public TextMeshProUGUI suppliesText;

    public Image threatFiller;
    public ScrollRect minionsScrollRect;
    public LayoutElement minionsScrollRectLE;

    public GameObject minionPrefab;
    public GameObject minionsHolderGO;
    public Transform minionsContentTransform;
    public Button upScrollButton;
    public Button downScrollButton;
    public MinionItem[] minionItems;

    public Toggle goalsToggle;
    public Toggle intelToggle;
    public Toggle inventoryToggle;
    public Toggle factionToggle;


    void Awake() {
        Instance = this;
    }

    public void UpdateUI() {
        manaText.text = "" + PlayerManager.Instance.player.currencies[CURRENCY.MANA];
        //redMagicText.text = "" + PlayerManager.Instance.player.redMagic;
        //greenMagicText.text = "" + PlayerManager.Instance.player.greenMagic;
        suppliesText.text = "" + PlayerManager.Instance.player.currencies[CURRENCY.SUPPLY];
        //threatFiller.fillAmount = (float) PlayerManager.Instance.player.threatLevel / 100f;
    }

    #region PlayerPicker
    public void ShowPlayerPickerAndPopulate() {
        if (PlayerManager.Instance.player.currentActiveAbility is ShareIntel) {
            UIManager.Instance.PopulatePlayerIntelsInPicker();
        } else if (PlayerManager.Instance.player.currentActiveAbility is GiveItem) {
            UIManager.Instance.PopulatePlayerItemsInPicker();
        } else if (PlayerManager.Instance.player.currentActiveAbility is TakeItem) {
            UIManager.Instance.PopulateLandmarkItemsInPicker();
        }
        UIManager.Instance.ShowPlayerPicker();
    }
    public void ShowPlayerPickerIntel() {
        PlayerManager.Instance.player.OnHidePlayerPicker();
        UIManager.Instance.PopulatePlayerIntelsInPicker();
        UIManager.Instance.ShowPlayerPicker();
    }
    public void ShowPlayerPickerInventory() {
        PlayerManager.Instance.player.OnHidePlayerPicker();
        UIManager.Instance.PopulatePlayerItemsInPicker();
        UIManager.Instance.ShowPlayerPicker();
    }
    public void ToggleIntelMenu(bool isOn) {
        if (isOn) {
            ShowPlayerPickerIntel();
        } else {
            UIManager.Instance.HidePlayerPicker();
        }
    }
    public void ToggleInventoryMenu(bool isOn) {
        if (isOn) {
            ShowPlayerPickerInventory();
        } else {
            UIManager.Instance.HidePlayerPicker();
        }
    }
    public void SetBottomMenuTogglesState(bool isOn) {
        goalsToggle.isOn = isOn;
        intelToggle.isOn = isOn;
        inventoryToggle.isOn = isOn;
        factionToggle.isOn = isOn;
    }
    #endregion

    #region Minions
    public void OnStartMinionUI() {
        OnScroll(Vector2.zero);
    }
    public void MinionDragged(ReorderableList.ReorderableListEventStruct reorderableListEventStruct) {
        MinionItem minionItem = reorderableListEventStruct.SourceObject.GetComponent<MinionItem>();
        minionItem.portrait.SetBorderState(true);
    }
    public void MinionCancel(ReorderableList.ReorderableListEventStruct reorderableListEventStruct) {
        MinionItem minionItem = reorderableListEventStruct.SourceObject.GetComponent<MinionItem>();
        minionItem.portrait.SetBorderState(false);
        //minionItem.SetEnabledState(false);
    }
    public void CollapseMinionHolder() {
        minionsHolderGO.GetComponent<TweenPosition>().PlayReverse();
    }
    public void UncollapseMinionHolder() {
        minionsHolderGO.GetComponent<TweenPosition>().PlayForward();
    }
    public void ScrollUp() {
        float y = minionsContentTransform.localPosition.y - 115f;
        if(y < 0f) {
            y = 0f;
        }
        minionsContentTransform.localPosition = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
    }
    public void ScrollDown() {
        float y = minionsContentTransform.localPosition.y + 115f;
        float height = minionsScrollRectLE.preferredHeight;
        if (y > height) {
            y = height;
        }
        minionsContentTransform.localPosition = new Vector3(minionsContentTransform.localPosition.x, y, minionsContentTransform.localPosition.z);
    }
    public void SortByLvlMinions() {
        PlayerManager.Instance.player.SortByLevel();
    }
    public void SortByTypeMinions() {
        PlayerManager.Instance.player.SortByType();
    }
    public void OnScroll(Vector2 vector2) {
        if (minionsContentTransform.localPosition.y == 0f) {
            //on top
            upScrollButton.gameObject.SetActive(false);
            downScrollButton.gameObject.SetActive(true);
        } else if (minionsContentTransform.localPosition.y == minionsScrollRectLE.preferredHeight) {
            //on bottom
            upScrollButton.gameObject.SetActive(true);
            downScrollButton.gameObject.SetActive(false);
        } else {
            upScrollButton.gameObject.SetActive(true);
            downScrollButton.gameObject.SetActive(true);
        }
    }
    #endregion
}
