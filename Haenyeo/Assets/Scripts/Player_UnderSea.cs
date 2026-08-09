using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
public class Player_UnderSea : Player
{
    Vector3 targetPosition;
    Vector3 tewakTargetPosition;
    Animator playerAnim;
    //public float rayLine = 3f;
    //public float poleRayLine;

    RaycastHit2D raycast;
    Fish fish;

    [Header("Tool")]
    [SerializeField]
    GameObject[] toolGuide;
    public GameObject toolGuideGroup;
    public ToolManager toolManager;

    [SerializeField]
    GameObject[] tools;

   // [SerializeField] Inventory inven;

    bool click = false;

    bool activeAttack = false;

    bool startSea = false; // 바다에 들어가자마자 y좌표 이상으로 올라와서 씬 이동 방지

    public AudioSource dive;


   // UnderSeaGameManager gm;
    protected override void Start()
    {
        y = 0;
        restrictY = 2f;
        base.Start();
        //currentHp = maxHp;
        playerAnim = GetComponent<Animator>();
        targetPosition = new Vector2(transform.position.x, camera.ViewportToWorldPoint(new Vector3(0, 0.5f, 0)).y);//.y - objectHeight*2f);
        transform.position = new Vector2(transform.position.x, camera.ViewportToWorldPoint(new Vector3(0, 1f, 0)).y); //+ objectHeight * 2f);
        tewakTargetPosition = new Vector2(transform.position.x, camera.ViewportToWorldPoint(new Vector3(0, 1f, 0)).y + objectHeight * 3f);
        StartCoroutine(StartUnderSea());
    }

    

    private void Update()
    {
        //Vector3 dir = render.flipX ? Vector3.right : Vector3.left;
        //if (toolManager.activeToolName == "Pole")
        //{
        //    rayLine = poleRayLine;
        //    raycast = Physics2D.CircleCast(transform.position, rayLine,Vector3.zero , 1, LayerMask.GetMask("Piscse"));
        //    Debug.DrawRay(rigid.position, dir * rayLine, Color.red);
        //}
        //else
        //{
        //    rayLine = 3f;

        //    Debug.DrawRay(rigid.position, dir * rayLine, Color.red);
        //    raycast = Physics2D.Raycast(transform.position, dir, rayLine, LayerMask.GetMask("Item") + LayerMask.GetMask("Piscse"));
        //}



        //if (raycast)
        //{
        //    fish = raycast.collider.GetComponent<Fish>();
        //    toolGuideGroup.SetActive(true);
        //    GuideSetting(raycast.collider.tag);
        //    if (toolManager.activeToolName == "Knife")
        //    {
        //        raycast.collider.transform.GetComponent<Fish>().ShowCanvas(gameObject.transform.position.x);
        //        activeAttack = true;
        //    }
        //    else if (toolManager.activeToolName == raycast.collider.tag)
        //    {
        //        raycast.collider.transform.GetComponent<Fish>().ShowCanvas(gameObject.transform.position.x);
        //        activeAttack = true;
        //    }
        //    else
        //    {
        //        raycast.collider.transform.GetComponent<Fish>().EnableCanvas();
        //        activeAttack = false;
        //    }

        //}
        //else
        //{
        //    if(fish!=null)
        //    {
        //        fish.EnableCanvas();
               
        //    }
        //    toolGuideGroup.SetActive(false);
        //    activeAttack = false;
        //}

        if (transform.position.y >= 4f || transform.position.y <= -35)
        {
            camera.transform.GetComponent<CameraController>().enabled = false;
            if (transform.position.y == topEdge)
                SceneManager.LoadScene("Sea");
            else if(transform.position.y == bottomEdge && SceneManager.GetActiveScene().name=="UnderSea")
            {
                SceneManager.LoadScene("UnderSea_2");
            }
        }
        else
            camera.transform.GetComponent<CameraController>().enabled = true;



    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;

    //    Gizmos.DrawWireSphere(transform.position, rayLine);
    //}

    //void GuideSetting(string coliderItemName)//, GameObject fish)
    //{
    //    for (int i = 0; i < toolGuide.Length; i++)
    //    {
    //        toolGuide[i].SetActive(toolGuide[i].name == coliderItemName);
    //    }

    //}



    IEnumerator StartUnderSea()
    {
        dive.Play();
        while (transform.position != targetPosition)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 2f * Time.deltaTime);
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        playerAnim.SetBool("Move", false);
        restrictY = 1f;
        startSea = true;
    }


 
    //public void Attack()
    //{
    //    playerAnim.SetTrigger(toolManager.activeToolName);
    //    Debug.Log(toolManager.activeToolName);
    //    //SoundManager.instance.PlaySE(toolManager.activeToolName);
    //    if(activeAttack)
    //    {
    //        fish.SetHP();
    //        if(fish.curHp<=0)
    //        {
    //            gm = FindObjectOfType<UnderSeaGameManager>();
    //            gm.CatchWindow(fish.transform.GetComponent<ItemPickUp>().item);
    //            if(fish!=null)
    //                inven.AcquireItem(fish.transform.GetComponent<ItemPickUp>().item);

    //            fish.Die();
    //        }
    //    }

    //}

}
