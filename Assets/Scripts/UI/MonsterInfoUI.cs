using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class MonsterInfoUI : UIMenu {

    [Space(10)]
    [Header("Content")]
    [SerializeField] private TextMeshProUGUI monsterInfoLbl;
    [SerializeField] private ScrollRect infoScrollView;

    internal Monster currentlyShowingMonster {
        get { return _data as Monster; }
    }

    internal override void Initialize() {
        base.Initialize();
        Messenger.AddListener(Signals.UPDATE_UI, UpdateMonsterInfo);
    }

    public override void OpenMenu() {
        base.OpenMenu();
        UpdateMonsterInfo();
    }
    public override void SetData(object data) {
        base.SetData(data);
        if (isShowing) {
            UpdateMonsterInfo();
        }
    }

    public void UpdateMonsterInfo() {
        if (currentlyShowingMonster == null) {
            return;
        }
        string text = string.Empty;
        text += "<b>Name:</b> " + currentlyShowingMonster.name;
        text += "\n<b>Party Power:</b> " + currentlyShowingMonster.computedPower;
        monsterInfoLbl.text = text;
        //infoScrollView.ResetPosition();
    }
}
