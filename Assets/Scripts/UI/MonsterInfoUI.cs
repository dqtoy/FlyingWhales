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

    //internal override void Initialize() {
    //    base.Initialize();
    //    //Messenger.AddListener(Signals.UPDATE_UI, UpdateMonsterInfo);
    //}

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
        text += "\n<b>Level:</b> " + currentlyShowingMonster.level;
        text += "\n<b>HP:</b> " + currentlyShowingMonster.currentHP + "/" + currentlyShowingMonster.maxHP;
        text += "\n<b>SP:</b> " + currentlyShowingMonster.currentSP + "/" + currentlyShowingMonster.maxSP;
        text += "\n<b>Str:</b> " + currentlyShowingMonster.strength + ", <b>Int:</b> " + currentlyShowingMonster.intelligence;
        text += "\n<b>Agi:</b> " + currentlyShowingMonster.agility + ", <b>Vit:</b> " + currentlyShowingMonster.vitality;
        text += "\n<b>Exp Drop:</b> " + currentlyShowingMonster.experienceDrop;

        text += "\n<b>P Final Attack:</b> " + currentlyShowingMonster.pFinalAttack;
        text += "\n<b>M Final Attack:</b> " + currentlyShowingMonster.pFinalAttack;
        text += "\n<b>Def:</b> " + currentlyShowingMonster.def;
        text += "\n<b>Crit Chance:</b> " + currentlyShowingMonster.critChance + "%";
        text += "\n<b>Dodge Chance:</b> " + currentlyShowingMonster.dodgeChance + "%";
        text += "\n<b>Computed Power:</b> " + currentlyShowingMonster.computedPower;

        text += "\n<b>Mode:</b> " + currentlyShowingMonster.currentMode.ToString();

        monsterInfoLbl.text = text;
        //infoScrollView.ResetPosition();
    }
}
