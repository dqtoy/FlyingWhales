using UnityEngine;
using System.Collections;

public class Healer : Role {
    private Plague _plague;

    public Healer(Citizen citizen): base(citizen){

    }
}
