
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    [SerializeField]
    GameObject joystick;
    
    [SerializeField]
    GameObject canvas_;

    [SerializeField]
    GameObject underSeaUI;

    [SerializeField] GameObject questBox;
    [SerializeField] TMP_Text questText;
    bool isquest = false;

    Canvas canvasComp;


    public Camera mainCamera;

    bool underSea = false;

    public GameObject phone;

    public string previousSceneName;

    public float time = 0;
    public float maxTime = 30;

    public TMP_Text timer;
    public float questTime =60;
    bool isQuestTime = false;
    public enum State { Idle, Afternoon, Night };
    public State state = State.Idle;

    //����Ŭ�� ����
    int ClickCount = 0;

    [Header("�����")]
    public SaveNLoad storage;
    public int index;

    [Header("Quest")]
    public TMP_Text count;
    int countNum;

    private void Awake()
    {
        if(instance ==null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        mainCamera = Camera.main;
        canvasComp = canvas_.GetComponent<Canvas>();
        SceneManager.sceneLoaded += OnSceneLoaded;
       
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
       
        if(isquest)
        {
            questTime -= Time.deltaTime;
            timer.text = ((int)questTime).ToString() + "s";
        }


        if (time <= maxTime / 3)
        {
            state = State.Idle;
        }
        else if (time <= maxTime / 3 * 2)
        {
            state = State.Afternoon;
        }
        else
        {
            state = State.Night;
        }

        if (underSea) 
        {
            //GameObject player = GameObject.FindGameObjectWithTag("Player");
           // SpriteRenderer render = player.GetComponent<SpriteRenderer>();
            if (time<maxTime)
                time += Time.deltaTime;

        }


        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClickCount++;
            if (!IsInvoking("DoubleClick"))
                Invoke("DoubleClick", 1.0f);

        }
        else if (ClickCount == 2)
        {
            CancelInvoke("DoubleClick");
            Application.Quit();
        }
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        //if(QuestSystem.Instance.ActiveQuests)
        //index = storage.saveData.nowIndex;
        Debug.Log(scene.name);
        mainCamera = Camera.main;
        canvasComp.worldCamera = mainCamera;
        if (scene.name != "Room" && scene.name != "Beach")
        {
            underSeaUI.SetActive(false);

            underSea = false;
            timer.gameObject.SetActive(false);

            if (scene.name == "UnderSea")
            {
                UnderSeaGameManager underGM = FindAnyObjectByType<UnderSeaGameManager>();
                underSea = true;
                underGM.storage = this.storage;

                // 제한 시간이 걸린 퀘스트가 진행 중이면 카운트다운을 띄운다
                foreach (var quest in QuestSystem.Instance.ActiveQuests)
                {
                    if (quest.TimeLimitSeconds <= 0 || count.gameObject.activeSelf)
                        continue;

                    count.gameObject.SetActive(true);
                    countNum = quest.TimeLimitSeconds;
                    count.text = countNum.ToString();
                    StartCoroutine(CountDown(countNum, underGM, quest));
                }
            }
            else
            {
                count.gameObject.SetActive(false);
            }

        }
        else
        {
            joystick.SetActive(false);  
        }

    }

    IEnumerator CountDown(int count, UnderSeaGameManager gm, Quest quest)
    {
        int getCount = count;
        while(getCount > 0)
        {
            this.count.text = getCount.ToString();
            getCount--;
            yield return new WaitForSeconds(1f);
        }

        this.count.gameObject.SetActive(false);
        gm.Tewak();

        // 시간 초과 -> 재도전. Register를 그냥 다시 부르면 진행 중이던 사본이 남아
        // 같은 퀘스트가 둘 활성화되고, 보고가 중복 처리된다.
        QuestSystem.Instance.Restart(quest);


    }

    public void ChangeScene(string sceneName)
    {
        SoundManager.instance.PlaySE("button");
        previousSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(sceneName);
    }

    public void CheckPhone()
    {
       // SoundManager.instance.PlaySE("UIClick");
        if (phone.activeSelf)
        {
            phone.SetActive(false);
        }
        else
            phone.SetActive(true);
    }

    public void ResetData()
    {
        storage.saveData.nowIndex = 0;
        storage.saveData.completeQuest = 0;
        storage.saveData.questAllCount = 0;
        storage.saveData.isQuest = false;
        storage.SaveData();
        SceneManager.LoadScene("Room");
    }

    void DoubleClick()
    {
        ClickCount = 0;
    }

    public void TimeSet()
    {
        Time.timeScale = 1f;
    }
}
