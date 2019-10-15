using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringNameplateItem : NameplateItem<string> {

    public string identifier { get; private set; }

    public string str { get; private set; }

    public override void SetObject(string o) {
        base.SetObject(o);
        str = o;
        identifier = string.Empty;
        mainLbl.text = str;
        subLbl.text = string.Empty;
    }

    public void SetIdentifier(string id) {
        identifier = id;
    }
}
