using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringNameplateItem : NameplateItem<string> {

    [SerializeField] private LocationPortrait _locationPortrait;
    
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
        if (identifier == "Landmark") {
            _locationPortrait.gameObject.SetActive(true);
            string landmarkName = str.Replace(' ', '_');
            LANDMARK_TYPE landmark = (LANDMARK_TYPE)Enum.Parse(typeof(LANDMARK_TYPE), landmarkName.ToUpper());
            _locationPortrait.SetPortrait(landmark);
            _locationPortrait.disableInteraction = true;
        } else {
          _locationPortrait.gameObject.SetActive(false);  
        }
    }
}
