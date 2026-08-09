using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Recipe : MonoBehaviour
{
    public Cooking[] cooking;

    public GameObject slotsParent;
    public GameObject recipeFrameParent;
    public GameObject framePrefab;

    RecipeSlot[] slots;

    public Transform ingredients;

    public GameObject recipeWindow;
    public GameObject fishWindow;


    private void Awake()
    {
        foreach(var item in cooking)
        {
            GameObject recipeSlot = Instantiate(framePrefab, recipeFrameParent.transform);
            recipeSlot.GetComponent<RecipeSlot>().AddRecipe(item);
            recipeSlot.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => temp(item));
            //foreach(var ingredient in item.needfood)
            //{
            //    GameObject ingredientSlot = Instantiate(framePrefab, ingredients.transform);
            //    ingredientSlot.GetComponent<RecipeSlot>().AddItem(ingredient)
            //}
               
        }

        slots = slotsParent.GetComponentsInChildren<RecipeSlot>();
 
    }

    void temp(Cooking cooking)
    {
        recipeWindow.SetActive(false);
        fishWindow.SetActive(true);
        if(ingredients.childCount!=0)
        {
            for (int i = 0; i < ingredients.childCount; i++)
                ingredients.GetChild(i).gameObject.SetActive(false);
        }

        for(int i=0; i< cooking.needfood.Length; i++)
        {
            if(i<ingredients.childCount)
            {
                ingredients.GetChild(i).gameObject.SetActive(true);
                ingredients.GetChild(i).GetComponent<RecipeSlot>().Test(cooking.needfood[i].food);
            }
            else
            {
                GameObject ingredientSlot = Instantiate(framePrefab, ingredients);
                ingredientSlot.GetComponent<RecipeSlot>().Test(cooking.needfood[i].food);
            }
        }


    }

    public void AddRecipeInfo()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].AddItem(cooking[0].needfood[i].food, cooking[0].needfood[i].needCount);
        }
    }


}
