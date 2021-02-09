using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] PlayerPanel panel = default;
    public event EventHandler<EventArgs> TurnEnded;
    protected static BitUtility bit;
    protected static PlayerLogic logic = PlayerLogic.Instance;
    protected GameData gameData;
    protected TextController text;

    public int ID { get; set; }
    public string Name { get; set; }
    public ulong BitSelectedHand { get; protected set; }
    public bool IsOver { get => BitPlayerCard == 0; }
    public bool HasPass { get; protected set; }

    public bool HasRevolusionalized { get; protected set; }

    public virtual void Render() { }

    public virtual void Open() { }
   
    public virtual void AddCard(int i)
    {
        var bitCard = 1ul << i;
        logic.AddCard(ID, bitCard);
    }

    public virtual void AddCards(List<CardData> cards)
    {
        cards.ForEach(c =>
        {
            var bitCard = CardUtility.ToBitCard(c.Suit, c.Number);

            if ((BitPlayerCard & bitCard) != 0) throw new Exception("Duplication Error");

            logic.AddCard(ID, bitCard);
        });
    }

    public virtual void OnTurn(ulong bitFieldCard, ulong bitUsedCard, int playingMember, 
        int playerMember, bool isRevolutionalizing)
    {
        gameData = new GameData(bitFieldCard, bitUsedCard, playingMember,
            playerMember, isRevolutionalizing);
    }

    public virtual void RenderText(string msg)
    {
        text.Render(msg);
    }

    public virtual void Clear()
    {
        logic.Clear(ID);
        text.Clear();
        BitSelectedHand = 0;
    }
    
    public void ChangePanelColor()
    {
        panel.ChangeColor();
    }

    public void RestorePanelColor()
    {
        panel.RestoreColor();
    }

    protected virtual void Awake()
    {
        bit = BitUtility.Instance;
        text = GetComponent<TextController>();
    }

    protected ulong BitPlayerCard
    {
        get => logic.GetBitPlayerCard(ID);
    }

    protected virtual void OnEndedTurn()
    {
        TurnEnded?.Invoke(this, EventArgs.Empty);
    }

    protected List<int> ToCardIDList(ulong bitCard)
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
