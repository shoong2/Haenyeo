using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsUtility : MonoBehaviour
{
    [ContextMenu("DeleteSaveData")]
    void DeleteSaveDate()
    {
        PlayerPrefs.DeleteAll();
    }
}
