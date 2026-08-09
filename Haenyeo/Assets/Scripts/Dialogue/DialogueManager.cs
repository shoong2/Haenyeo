using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] GameObject go_DialogueBar;

    [SerializeField] TMP_Text txt_Dialogue;
    [SerializeField] TMP_Text txt_Name;

    [Header("텍스트 출력 딜레이")]
    [SerializeField] float textDelay;

    [Header("저장소")]
    [SerializeField] SaveNLoad storage;

    [Header("알림창")]
    [SerializeField] Image itemBox;

    [Header("대화 캐릭터 이미지")]
    [SerializeField] Image[] charImg;

    Dialogue[] dialogues;

    bool isDialogue;
    bool isNext;
    bool next;

    int lineCount;
    int contextCount;

    QuestReporter reporter;
    Coroutine typewriterCo;

    int npcLayerMask;
    Dictionary<string, GameObject> charImgByName;

    void Awake()
    {
        npcLayerMask = LayerMask.GetMask("NPC");

        charImgByName = new Dictionary<string, GameObject>(charImg.Length);
        for (int i = 0; i < charImg.Length; i++)
            charImgByName[charImg[i].name] = charImg[i].gameObject;
    }

    void ShowDialogueImg(string charName, bool active = true)
    {
        if (!active)
        {
            for (int i = 0; i < charImg.Length; i++)
                charImg[i].gameObject.SetActive(false);
            return;
        }

        foreach (var kv in charImgByName)
            kv.Value.SetActive(kv.Key == charName);
    }

    public void ShowDialogue(Dialogue[] p_dialogues)
    {
        isDialogue = true;
        txt_Dialogue.text = "";
        txt_Name.text = "";

        dialogues = p_dialogues;
        StartTypewriter();
    }

    void StartTypewriter()
    {
        if (typewriterCo != null) StopCoroutine(typewriterCo);
        typewriterCo = StartCoroutine(Typewriter());
    }

    IEnumerator Typewriter()
    {
        SettingUI(true);

        string t_ReplaceText = dialogues[lineCount].contexts[contextCount]
            .Replace("'", ",")
            .Replace("\\n", "\n");

        txt_Name.text = dialogues[lineCount].name[contextCount];
        ShowDialogueImg(txt_Name.text);

        var wait = new WaitForSeconds(textDelay);
        for (int i = 0; i < t_ReplaceText.Length; i++)
        {
            if (next)
            {
                txt_Dialogue.text = t_ReplaceText;
                next = false;
                break;
            }

            txt_Dialogue.text = t_ReplaceText.Substring(0, i + 1);
            yield return wait;
        }

        isNext = true;
        typewriterCo = null;
    }

    void SettingUI(bool p_flag)
    {
        go_DialogueBar.SetActive(p_flag);
    }

    void Update()
    {
        if (isDialogue)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!isNext)
                {
                    next = true;
                }
                else
                {
                    isNext = false;
                    txt_Dialogue.text = "";

                    if (++contextCount < dialogues[lineCount].contexts.Length)
                    {
                        StartTypewriter();
                    }
                    else
                    {
                        contextCount = 0;
                        if (++lineCount < dialogues.Length)
                            StartTypewriter();
                        else
                            EndDialogue();
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                contextCount = dialogues[lineCount].contexts.Length - 2;
            }

            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f, npcLayerMask);

            if (!hit) return;

            foreach (var quest in QuestSystem.Instance.ActiveQuests)
            {
                if (quest.CurrentTaskGroup.ContainsTarget(hit.collider.tag))
                {
                    reporter = hit.collider.gameObject.GetComponent<QuestReporter>();
                    ShowDialogue(DatabaseManager.instance.GetDialogue(storage.saveData.nowIndex));
                    break;
                }
            }
        }
    }

    public void EndDialogue()
    {
        isDialogue = false;
        contextCount = 0;
        lineCount = 0;
        dialogues = null;
        isNext = false;
        SettingUI(false);

        if (typewriterCo != null)
        {
            StopCoroutine(typewriterCo);
            typewriterCo = null;
        }

        ShowDialogueImg("stop", false);

        if (reporter != null)
        {
            reporter.Report();
            reporter = null;
        }
    }
}
