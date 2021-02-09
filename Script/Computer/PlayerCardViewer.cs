using System.Collections.Generic;
using UnityEngine;

public class PlayerCardViewer : MonoBehaviour
{
    [SerializeField] Card cardPrefab = default;
    [SerializeField] bool isCardFace = default;
    protected List<Card> cards;

    public virtual void Render(List<int> idList)
    {
        Clear();

        idList.ForEach(i => cards.Add(CreateCard(i)));
    }

    public void Clear()
    {
        foreach (Transform c in transform)
        {
            Destroy(c.gameObject);
        }

        cards.Clear();
    }

    public void Open()
    {
        cards.ForEach(c => c.IsFace = true);
    }

    private void Awake()
    {
        cards = new List<Card>();
    }

    protected virtual Card CreateCard(int i)
    {
        var card = Instantiate(cardPrefab, transform, false);
        card.ID = i;
        card.IsFace = isCardFace;

        return card;
    }
}
