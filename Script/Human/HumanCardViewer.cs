using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class HumanCardViewer : PlayerCardViewer
{
    public event EventHandler<EventArgs> SelectedCardChanged;
    
    public List<Card> SelectedCard
    {
        get => cards.Where(c => c.HasSelected).ToList<Card>();
    }

    public void EmphasizeLegalCard(List<int> legalCardList)
    {
        cards.ForEach(c => c.IsLegal = false);

        legalCardList.ForEach(d =>
        {
            var card = cards.FirstOrDefault(c => c.ID == d);

            if (card != null)
            {
                card.IsLegal = true;
            }
        });
    }

    public void Cancel()
    {
        cards.ForEach(c => c.HasSelected = false);
    }

    protected override Card CreateCard(int i)
    {
        var card = base.CreateCard(i);

        card.IsLegal = true;
        card.SelectedCardChanged +=
            (s, e) => { SelectedCardChanged?.Invoke(this, EventArgs.Empty); };

        return card;
    }
}
