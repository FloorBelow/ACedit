using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACSharp;
public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        Forge forge = new Forge(@"F:\Games\Assassin's Creed 1\DataPC_SolomonTemple.forge");
        for(int i = 0; i < forge.datafileTable.Length; i++) {
            Debug.Log(forge.datafileTable[i].name);
        }
        //Debug.Log(ACConsole.consoleText);
    }
}
