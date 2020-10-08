using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class BattleLog : MonoBehaviour
{
    public void CreateText(string message) {
        //Path of the File
        string path = Application.dataPath + "/Text/Log.txt";
        Debug.Log(path);
        //Create File if it doesn't exist
        if (!File.Exists(path)){
            File.WriteAllText(path, "Combat_ID,Total_Turn_Number,Character_Turn_Number,Player_Or_NPC,ATTACKER,TARGET,MOVE,Hit_Number,Damage_or_Healing,Targets_Current_HP,Target Max HP,Hasted_Flag,Haste_Value,Damage_Buff_Flag,Damage_Buff_Value,Target_damage_Taken_increase_flag,Target_damage_Taken_increase_amount \n");
        }

        //Content of the File
        //string content = "Log time: " + System.DateTime.Now + "\n";
        //Add some text to it
        // File.AppendAllText(path, content + " | " + message);
        File.AppendAllText(path, message);
    }


}
