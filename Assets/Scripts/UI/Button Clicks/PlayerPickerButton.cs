using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPickerButton : MonoBehaviour {
    public Text buttonText;
    IPlayerPicker _playerPicker;
	
    public void SetPlayerPicker(IPlayerPicker playerPicker) {
        _playerPicker = playerPicker;
        buttonText.text = _playerPicker.thisName;
    }

    public void OnClickPlayerPicker() {
        PlayerManager.Instance.player.SetCurrentlySelectedPlayerPicker(_playerPicker);
    }
}
