using System;
using System.Collections.Generic;
using UnityEngine;

public class FieldController : MonoBehaviour
{
    private static BitUtility bit;
    private FieldCardViewer viewer;

    public ulong BitFieldCard { get; private set; }
    public ulong BitUsedCard { get; private set; }

    public void AddCard(ulong bitCard)
    {
        BitFieldCard = bitCard;
        BitUsedCard |= bitCard;

        viewer.Render(ToCardIDList(BitFieldCard));
    }

    public void AddCards(List<CardData> cards)
    {
        cards.ForEach(c =>
        {
            var bitCard = CardUtility.ToBitCard(c.Suit, c.Number);

            if ((BitFieldCard & bitCard) != 0) throw new Exception("Duplication Error");

            BitFieldCard |= bitCard;
        });

        viewer.Render(ToCardIDList(BitFieldCard));
    }

    public void AddUsedCards(List<CardData> cards)
    {
        cards.ForEach(c =>
        {
            var bitCard = CardUtility.ToBitCard(c.Suit, c.Number);

            if ((BitUsedCard & bitCard) != 0) throw new Exception("Duplication Error");

            BitUsedCard |= bitCard;
        });
    }

    public void Clear()
    {
        BitFieldCard = 0;
        viewer.Clear();
    }

    private void Awake()
    {
        bit = BitUtility.Instance;
        viewer = GetComponent<FieldCardViewer>();
    }

    private List<int> ToCardIDList(ulong bitCard)
    {
        var list = new List<int>();

        while (bitCard != 0)
        {
            var i = bit.BitScanForward(bitCard);
            list.Add(i);
            bitCard &= bitCard - 1;
        }

        return list;
    }
}
