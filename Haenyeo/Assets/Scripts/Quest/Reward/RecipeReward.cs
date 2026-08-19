using UnityEngine;

// 기획 시트의 "퀘스트 리워드 #6 - Recipe". 레시피를 해금한다.
// 레시피 시스템이 없어서 아직 적용하지 않는다.
[CreateAssetMenu(menuName = "Quest/Reward/Recipe", fileName = "RecipeReward_")]
public class RecipeReward : Reward
{
    [Tooltip("시트의 레시피 ID (100001~)")]
    [SerializeField]
    int recipeId;

    public int RecipeId => recipeId;

    public override void Give(Quest quest)
    {
        // TODO: 레시피 시스템이 생기면 여기서 해금한다.
        Debug.Log($"[RecipeReward] 미적용 - recipeId {recipeId}");
    }
}
