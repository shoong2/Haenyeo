using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UnderSeaGameManager : MonoBehaviour
{
    public GameObject player;

    [Header("Distance")]
    public TMP_Text distaceText;
    public float distance;
    public float num; //오차 줄이기 숫자
    float distanceNum;

    SpriteRenderer render;
    Camera camera;

    [Header("HP")]
    public GameObject seaHP;
    public Image hpSlider;
    public float hpX = 2f; //hp x위치
    public float maxHp = 10f;
    public float currentHp;
    bool activeHp =true;

    [Header("알림창")]
    public GameObject window;
    public TMP_Text itemText;
    public Image itemImage;

    float playerStartPos;
    public float upSideSpeed;
   // public bool startQuestIndex_1 = false;
    public SaveNLoad storage;
    // Start is called before the first frame update
    void Start()
    {
        distanceNum = distance;
        //distance = 0;
        currentHp = maxHp;
        camera = Camera.main;
        render = player.GetComponent<SpriteRenderer>();
        playerStartPos = player.transform.position.y + player.transform.localScale.y/10;
        Debug.Log(playerStartPos);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(player.transform.position.y - playerStartPos);
        distance = -(player.transform.position.y - playerStartPos)/9;
        distaceText.text =(distanceNum+ distance).ToString("F1")+"M";
        
        if(storage && storage.saveData.nowIndex==1 && distance>=3)
        {
            storage.saveData.nowIndex++;
            //QuestManager.completeQuest = true;
            Debug.Log("complete quest1");
        }    

        seaHP.transform.position = player.transform.position + new Vector3(render.flipX ? -hpX : hpX, 0 , 0); // hp 따라다니기
        //currentHp -= Time.deltaTime;
        hpSlider.fillAmount = currentHp / maxHp;

        if (currentHp < 0 && activeHp)
        {
            activeHp = false;
            StartCoroutine(TewakMove());
        }
        else
            currentHp -= Time.deltaTime;
    }

    public void Tewak()
    {
        StartCoroutine(TewakMove());
    }
    IEnumerator TewakMove() // 죽었을 때와 같이 쓰기
    {
        //SoundManager.instance.PlaySE("Tewak");
        if (!activeHp)
        {
            player.GetComponent<Animator>().SetTrigger("Die");
        }
        else
            player.GetComponent<Animator>().SetTrigger("Tewak");

        activeHp = false;
        seaHP.SetActive(false);
        Vector3 tewakTargetPosition = new Vector2(player.transform.position.x, camera.ViewportToWorldPoint(new Vector3(0, 1f, 0)).y + 3f);
        //while (player_UnderSea.transform.position != tewakTargetPosition)
        while (player.transform.position != tewakTargetPosition)
        {
            player.transform.position = Vector3.MoveTowards(player.transform.position, tewakTargetPosition, upSideSpeed * Time.deltaTime);
            yield return null;
        }
        SceneManager.LoadScene("Sea");
        //GameManager.instance.ChangeScene("Sea");
    }

    public void TimeControl()
    {
        if (Time.timeScale == 0)
            Time.timeScale = 1;
        else
            Time.timeScale = 0;
    }

    public void CatchWindow(Item item_)
    {
        window.SetActive(true);
        itemText.text = "<b>[" +item_.itemName + "]</b>" + "을\n채집했습니다!";
        itemImage.sprite = item_.itemImage;
    }

    

}
