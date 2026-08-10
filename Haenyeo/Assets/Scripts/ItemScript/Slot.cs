using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Slot : MonoBehaviour
{
    public Item item; //획득한 아이템
    public Image itemImage; // 아이템의 이미지
    public int itemCount;

    [SerializeField]
    public TMP_Text text_Count;
    [SerializeField]
    GameObject countImage;

    //[SerializeField]
    //GameObject go_CountImage; // 아이템 카운트 이미지 필요할 경우 활성화


    //이미지 투명도 조절
    protected void SetColor(float _alpha)
    {
        Color color = itemImage.color;
        color.a = _alpha;
        itemImage.color = color;
    }


    //아이템 획득
    public virtual void AddItem(Item _item, int _count =1)
    {
        item = _item;
        itemCount = _count;
        itemImage.sprite = item.itemImage;

        if (item.itemType == Item.ItemType.item)
        {
            text_Count.text = itemCount.ToString();
            //text_Count.text = "0";
            if(countImage!=null)
                countImage.SetActive(true);
            
        }
        else
        {
            if(countImage!=null)
                countImage.SetActive(false);
            //text_Count.text = "0";
            text_Count.gameObject.SetActive(false);
        }
        SetColor(1);
    }


    //아이템 개수 조정
    public void SetSlotCount(int _count)
    {
        itemCount += _count;
        text_Count.text = itemCount.ToString();

        if(itemCount <=0)
        {
            ClearSlot();
        }
    }


    //슬롯 초기화
    void ClearSlot()
    {
        item = null;
        itemCount = 0;
        itemImage.sprite = null;
        SetColor(0);

        text_Count.text = "0";
        //text_Count.gameObject.SetActive(false);

    }
}
