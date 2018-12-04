using BayatGames.SaveGameFree.Types;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadSaveData {

    public int squadID;
    public int leaderID;
    public string squadName;
    public Dictionary<int, ICHARACTER_TYPE> memberIDs;
    public int emblemBGIndex;
    public int emblemIndex;
    public ColorSave squadColor;

    public SquadSaveData(Squad squad) {
        squadID = squad.id;
        squadName = squad.name;
        if (squad.squadLeader == null) {
            leaderID = -1;
        } else {
            leaderID  = squad.squadLeader.id;
        }

        memberIDs = new Dictionary<int, ICHARACTER_TYPE>();
        for (int i = 0; i < squad.squadMembers.Count; i++) {
            Character currMember = squad.squadMembers[i];
            memberIDs.Add(currMember.id, currMember.icharacterType);
        }

        emblemBGIndex = CharacterManager.Instance.GetEmblemBGIndex(squad.emblemBG);
        emblemIndex = CharacterManager.Instance.GetEmblemIndex(squad.emblem);
        squadColor = new ColorSave(squad.squadColor);
    }
}
