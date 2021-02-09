using System.Linq;
using UnityEngine;

public class PlayerLogic
{
    private static BitUtility bit;
    private static ulong[] bitPlayerCards;

    public static PlayerLogic Instance { get; } = new PlayerLogic();

    public ulong GetBitPlayerCard(int id)
    {
        return bitPlayerCards[id];
    }

    public void AddCard(int id, ulong bitCard)
    {
        bitPlayerCards[id] |= bitCard;
    }

    public ulong SelectBitHand(int id, GameData gameData)
    {
        if (gameData.BitFieldCard == 0)
        {
            return SelectBitHandOnDealer(id, gameData);
        }
        else
        {
            return SelectBitHandOnNoneDealer(id, gameData);
        }
    }

    public void DrawCard(int id, ulong hand)
    {
        bitPlayerCards[id] ^= hand;
    }

    public void Clear(int id)
    {
        bitPlayerCards[id] = 0;
    }

    private PlayerLogic()
    {
        bit = BitUtility.Instance;
        bitPlayerCards = new ulong[4];
    }

    //親のときに役を選択
    private ulong SelectBitHandOnDealer(int id, GameData gameData)
    {
        var bitPlayerCard = bitPlayerCards[id];
        var playerCard = new PlayerCard(bitPlayerCard, gameData);
        var hands = playerCard.Hands;
        
        var ordered = hands.OrderByDescending(h => h.Priority).ThenBy(h => h.Rank)
            .ToList<Hand>();

        return ordered[0].BitHand;
    }

    //子のときに役を選択
    private ulong SelectBitHandOnNoneDealer(int id, GameData gameData)
    {
        var bitPlayerCard = bitPlayerCards[id];
        var playerCard = new PlayerCard(bitPlayerCard, gameData);
        var legalHands = playerCard.GetLegalHands(gameData);
        
        if (bit.IsSequence(gameData.BitFieldCard))
        {
            legalHands.ForEach(h =>
            {
                var next = playerCard.CreateNextPlayerCard(h.BitHand, gameData);

                if (next.CanEndGame || playerCard.Score <= next.Score + 1)
                {
                    h.Selected = true;
                }
            });
        }
        else
        {
            for (int i = 0; i < legalHands.Count; i++)
            {
                var h = legalHands[i];
                var next = playerCard.CreateNextPlayerCard(h.BitHand, gameData);

                if (h.CanEndTurn && playerCard.CanEndGame)
                {
                    h.Selected = true;
                }
                else if (h.CanEndTurn)
                {
                    if (playerCard.Hands.Count < 4 ||
                        next.Hands.Exists(h2 => h2.CanEndTurn))
                    {
                        h.Selected = true;
                    }
                }
                else if (next.Hands.Count == 1 &&
                    next.Hands[0].Score <= 50)
                {
                    continue;
                }
                else if (next.CanEndGame)
                {
                    h.Selected = true;
                }
                else if (IsPrettyHigh(h.BitHand, gameData.IsRevolutionalizing) &&
                    next.Hands.Count >= 4 && next.Score > 0)
                {
                    continue;
                }
                else if (next.Score <= playerCard.Score)
                {
                    h.Selected = true;
                }
            }
        }

        var selected = legalHands.Where(h => h.Selected).ToList<Hand>();

        if (selected.Count == 0) return 0;

        var min = Mathf.Infinity;
        int m = 0;

        for (int i = 0; i < selected.Count; i++)
        {
            var next = playerCard.CreateNextPlayerCard(selected[i].BitHand, gameData);
            
            if (next.Score <= min)
            {
                if (next.Score == min)
                {
                    if (selected[i].Rank < selected[m].Rank) m = i;
                }
                else
                {
                    m = i;
                }

                min = next.Score;
            }
        }

        return selected[m].BitHand;
    }

    //Kと1(革命時は4と5)の単体やグループかどうか判定する
    private bool IsPrettyHigh(ulong bitHand, bool isRevolutionalizing)
    {
        var qpCard = bit.ToQPCard(bitHand);

        if (!isRevolutionalizing)
        {
            return (qpCard & 0x0000110000000000ul) > 0;
        }
        else
        {
            return (qpCard & 0x0000000000000110ul) > 0;
        }
    }
}
