using System;
public enum InteractionType
{
    Interact,
    Cut
}

public static class PlayerInteractionEvent
{
    public static event Action<BaseCounter, Player, InteractionType> OnInteraction;

    public static void RaiseInteraction(BaseCounter counter, Player player, InteractionType type)
    {
        OnInteraction?.Invoke(counter, player, type);
    }
}