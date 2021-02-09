using System.Collections.Generic;
using UnityEngine;

public class FieldCardViewer : MonoBehaviour
{
    [SerializeField] Card cardPrefab = default;
    private readonly int[] dxs = new int[] { -30, 0, 30 };
    private readonly int[] das = new int[] { 0, 20, -20 };
    private int renderCount;

    public void Render(List<int> idList)
    {
        int x = 0;
        int r = renderCount % 3;

        idList.ForEach(i =>
        {
            var card = CreateCard(i);
            card.RotationZ = das[r];
            card.PositionX = dxs[r] + x;
            x += 20;
        });

        renderCount++;
    }

    public void Clear()
    {
        renderCount = 0;

        foreach (Transform c in transform)
        {
            Destroy(c.gameObject);
        }
    }

    private void Awake()
    {
        renderCount = 0;
    }

    private Card CreateCard(int i)
    {
        var card = Instantiate(cardPrefab, transform, false);
        card.ID = i;

        return card;
    }
}
