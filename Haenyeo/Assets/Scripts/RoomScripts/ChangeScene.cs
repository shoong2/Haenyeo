using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ChangeScene : MonoBehaviour
{
    Inventory inven;

   public void GoSea()
    {
        GameManager.instance.ChangeScene("Sea");
    }

    public void GoBeach()
    {
        GameManager.instance.ChangeScene("Beach");
    }

    public void GoKitchen()
    {
        SceneManager.LoadScene("Kitchen");
    }

    public void GoTitle()
    {
        SceneManager.LoadScene("Title");
    }

    public void StartGame()
    {
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("Room");
    }
    public void GoMiniGame()
    {
        SceneManager.LoadScene("MiniGame");
    }

    //In the beach scene, bag button 
    public void BagButton()
    {
        if (inven != null)
            inven = FindObjectOfType<Inventory>();

        inven.transform.GetChild(0).gameObject.SetActive(true);
    }
}
