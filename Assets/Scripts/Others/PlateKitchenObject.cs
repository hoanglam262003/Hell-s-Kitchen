using System;
using System.Collections.Generic;
using UnityEngine;

public class PlateKitchenObject : KitchenObject
{
    public event EventHandler<IngredientAddedEventArgs> OnIngredientAdded;
    public class IngredientAddedEventArgs : EventArgs
    {
        public KitchenObjectSO kitchenObjectSO;
    }
    [SerializeField]
    private List<KitchenObjectSO> validKitchenObjectsList;
    private List<KitchenObjectSO> kitchenObjectsList;
    private void Awake()
    {
        kitchenObjectsList = new List<KitchenObjectSO>();
    }
    public bool TryAddIngredient(KitchenObjectSO kitchenObjectSO)
    {
        if (!validKitchenObjectsList.Contains(kitchenObjectSO))
        {
            return false;
        }
        if (kitchenObjectsList.Contains(kitchenObjectSO))
        {
            return false;
        } else
        {
            kitchenObjectsList.Add(kitchenObjectSO);
            OnIngredientAdded?.Invoke(this, new IngredientAddedEventArgs
            {
                kitchenObjectSO = kitchenObjectSO
            });
            return true;
        }
    }
}
