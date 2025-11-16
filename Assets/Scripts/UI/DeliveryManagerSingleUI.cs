using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryManagerSingleUI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI recipeNameText;
    [SerializeField]
    private Transform iconContainerTransform;
    [SerializeField]
    private Transform iconTemplateTransform;

    private void Awake()
    {
        iconTemplateTransform.gameObject.SetActive(false);
    }

    public void SetRecipeSO(RecipeSO recipeSO)
    {
        recipeNameText.text = recipeSO.recipeName;

        foreach (Transform child in iconContainerTransform)
        {
            if (child == iconTemplateTransform) continue;
            Destroy(child.gameObject);
        }

        foreach (KitchenObjectSO kitchenObjectSO in recipeSO.kitchenObjectSOList)
        {
            Transform iconTransform = Instantiate(iconTemplateTransform, iconContainerTransform);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<Image>().sprite = kitchenObjectSO.sprite;
        }
    }
}
