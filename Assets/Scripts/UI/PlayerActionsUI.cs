using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerActionsUI : UIMenu {

    public GameObject playerActionsBtnPrefab;
    public Transform playerActionsContentTransform;

    public BaseLandmark currentlyShowingLandmark {
        get { return _data as BaseLandmark; }
    }

    public override void OpenMenu() {
        ShowMenu();
        UpdatePlayerActions();
    }
    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdatePlayerActions();
        }
    }

    #region Utilities
    public void UpdatePlayerActions() {
        Reposition();
    }
    private void Reposition() {
        Vector3 pos = Input.mousePosition;
        this.transform.position = pos;
	}
    #endregion
}
