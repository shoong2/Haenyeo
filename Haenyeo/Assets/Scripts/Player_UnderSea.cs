using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Player_UnderSea : Player
{
    Vector3 targetPosition;
    Vector3 tewakTargetPosition;
    Animator playerAnim;
    public float rayLine = 3f;
    public GameObject seaHP;
    //[SerializeField]
    //GameObject hp;
    //[SerializeField]
    //Image hpSlider;
    //public float maxHp = 10f;
    //float currentHp;
    //public float hpX =2f;

    RaycastHit2D raycast;

    [SerializeField]
    GameObject toolGuide;

    [SerializeField]
    GameObject[] tools;

    [SerializeField] Inventory inven;

    bool click = false;

    bool test = false;

    public GameObject getBox;
    public TMP_Text getText;
    public Image getSprite;
    protected override void Start()
    {
        Debug.Log("start?");
        y = 0;
        restrictY = 2f;
        base.Start();
        playerAnim = GetComponent<Animator>();
        targetPosition = new Vector2(transform.position.x, GameManager.instance.mainCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).y);//.y - objectHeight*2f);
        transform.position = new Vector2(transform.position.x, GameManager.instance.mainCamera.ViewportToWorldPoint(new Vector3(0, 1f, 0)).y + objectHeight*2f);
        tewakTargetPosition = new Vector2(transform.position.x, GameManager.instance.mainCamera.ViewportToWorldPoint(new Vector3(0, 1f, 0)).y + objectHeight * 3f);
        StartCoroutine(StartUnderSea());
    }

    

    private void Update()
    {
        Vector3 dir = render.flipX ? Vector3.right : Vector3.left;
        Debug.DrawRay(rigid.position, dir * rayLine, Color.red);
        raycast = Physics2D.Raycast(transform.position, dir, rayLine, LayerMask.GetMask("Item"));


        if (raycast)
        {
            //if (!test)
            //{
            //    GameObject seahp = Instantiate(seaHP, raycast.collider.transform.position,
            //      Quaternion.identity, GameObject.Find("UnderSea").transform);
            //    seahp.transform.position += new Vector3(2, 0, 0);
            //    test = true;
            //}

            if (raycast.collider.tag == "Knife")
            {
            
                GuideSetting(0, raycast.collider.gameObject);
                raycast.collider.transform.GetComponent<Fish>().ShowCanvas();
                //GameObject seahp = Instantiate(seaHP, raycast.collider.transform.position,
                //    Quaternion.identity, GameObject.Find("UnderSea").transform);

            }
            else if (raycast.collider.tag == "Hoe")
            {
                GuideSetting(1, raycast.collider.gameObject);
                raycast.collider.transform.GetComponent<Fish>().ShowCanvas();
            }
            else if (raycast.collider.tag == "Pole")
            {
                //toolGuide.SetActive(true);
                //toolGuide.transform.position = tools[2].transform.position;
                //toolGuide.transform.parent = tools[2].transform;
                GuideSetting(2, raycast.collider.gameObject);
                raycast.collider.transform.GetComponent<Fish>().ShowCanvas();
            }
            //Debug.Log("det");
        }
        else
        {
            toolGuide.SetActive(false);
           // raycast.collider.transform.GetComponent<Fish>().canvas.SetActive(false);
        }

        if (transform.position.y >= 0.5f || transform.position.y <= -35)
        {
            GameManager.instance.mainCamera.transform.GetComponent<CameraController>().enabled = false;
        }
        else
            GameManager.instance.mainCamera.transform.GetComponent<CameraController>().enabled = true;


    }

    void GuideSetting(int index, GameObject fish)
    {
        toolGuide.SetActive(true);
        toolGuide.transform.position = tools[index].transform.position;
        toolGuide.transform.parent = tools[index].transform;
        if(click == true)
        {
            if (fish.transform.GetComponent<Fish>().curHp > 0)
            {
                fish.transform.GetComponent<Fish>().SetHP();
                if(fish.transform.GetComponent<Fish>().curHp ==0)
                {
                    Destroy(fish.gameObject);
                    getText.text = "<b>[" + fish.transform.GetComponent<ItemPickUp>().item.itemName + "]</b> 을\n채집했습니다!";
                    getSprite.sprite = fish.transform.GetComponent<ItemPickUp>().item.itemImage;
                    inven.AcquireItem(fish.transform.GetComponent<ItemPickUp>().item);
                    getBox.SetActive(true);
                }
                click = false;
            }
            //else
            //{
            //    getText.text = "<b>[" + fish.transform.GetComponent<ItemPickUp>().item.itemName + "]</b> 을\n채집했습니다!";
            //    getSprite.sprite = fish.transform.GetComponent<ItemPickUp>().item.itemImage;
            //    inven.AcquireItem(fish.transform.GetComponent<ItemPickUp>().item);
            //    getBox.SetActive(true);
            //}
        }
    }

    public void Attack()
    {
        if(toolGuide.activeSelf)
        {

        }
    }


    IEnumerator StartUnderSea()
    {
        //currentHp = maxHp;
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 2f * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        //playerAnim.SetTrigger("Swim");
        playerAnim.SetBool("Move", false);
        restrictY = 1f;

    }

    
    public void Hoe()
    {
        playerAnim.SetTrigger("Hoe");
        click = true;
    }
    
    public void Knife()
    {
        playerAnim.SetTrigger("Knife");
        click = true;
    }

    public void Pole()
    {
        playerAnim.SetTrigger("Pole");
        click = true;
        //playerAnim.SetBool("test", true);
    }

}
