using System;
using UnityEngine;

public class Hand
{
    private static readonly ulong MAll = 0x000fffffffffffff;
    private static BitUtility bit;
    private readonly ulong bitHand;
    private readonly int sameQuantity;
    private readonly int rank;

    public ulong BitHand { get => bitHand; }

    //場を流せる役かどうか
    public bool CanEndTurn { get => Score >= 86; }

    public bool Selected { get; set; }

    public int Rank { get => rank; }

    //役の構成枚数
    public int Quantity { get => bit.CountBit(bitHand); }
    
    //役評価値
    public float Score { get; private set; }

    //優先評価値
    public float Priority { get; set; }

    public int EnemyHigherRankQuantity { get; private set; }

    public bool CanRevolutionalize
    {
        get
        {
            var isOver3 = bit.IsGroup(bitHand, 4);
            
            int cnt = bit.CountBit(BitHand);
            var isOver4 = cnt >= 5 && bit.IsSequence(bitHand, cnt);

            return isOver3 || isOver4;
        }
    }

    public Hand(ulong bitHand, ulong playerCard, GameData gameData, int same = 0)
    {
        this.bitHand = bitHand;
        bit = BitUtility.Instance;

        //役のランクを求める。革命時は逆になる
        rank = gameData.IsRevolutionalizing ?
            12 - (int)(bit.BitScanForward(bitHand) / 4) :
            (int)(bit.BitScanReverse(bitHand) / 4);

        //場に出ていないカードを取得
        var enemyCard = ~(playerCard | gameData.BitUsedCard) & MAll;
        //場に出ていないカードの中で、役より強いカードを取得
        var enemyHigherCard = bit.GetHigherBitCard(enemyCard, bitHand,
            gameData.IsRevolutionalizing);
        EnemyHigherRankQuantity = CountRank(enemyHigherCard);
        
        sameQuantity = same;
        
        Score = EvaluateHand(enemyHigherCard, gameData);
    }

    //役評価値を計算
    public float EvaluateHand(ulong higher, GameData gameData)
    {
        float score = 0f;

        if (Quantity == 1)
        {
            score = EvaluateSingleHand();
        }
        else if (bit.IsSequence(bitHand, Quantity))
        {
            score = EvaluateSequenceHand(higher, Quantity, gameData);
        }
        else if (bit.IsGroup(bitHand, Quantity))
        {
            score = EvaluateGroupHand(higher, Quantity, gameData);
        }

        return Mathf.Max(score, 1);
    }

    //優先評価値を求める
    public void SetPriority(bool canEndGame)
    {
        if (canEndGame)
        {
            if (!CanEndTurn)
            {
                Priority = 2;
                return;
            }
        }

        if (CanEndTurn)
        {
            Priority = 110 - Score;
        }
        else
        {
            Priority = 40 + (13 - Rank) * 2
                + sameQuantity * 4;
        }
    }

    public override string ToString()
    {
        string s = "";
        var bh = bitHand;

        while (bh != 0)
        {
            int i = bit.BitScanForward(bh);
            s += CardUtility.ToCardString(i) + ", ";
            bh ^= 1ul << i;
        }

        return s.TrimEnd(new char[] { ',', ' ' });
    }

    //単体のカードの強さ評価値を計算
    private float EvaluateSingleHand()
    {
        return 100 - EnemyHigherRankQuantity * 30;
    }

    //階段の強さ評価値を計算
    private float EvaluateSequenceHand(ulong higher, int cnt, GameData gameData)
    {
        var seqs = bit.GetSequenceList(higher, cnt);

        return 100 - seqs.Count * (gameData.PlayerNumber + 1 - gameData.PlayingNumber);
    }

    //グループの強さの評価値を計算
    private float EvaluateGroupHand(ulong higher, int cnt, GameData gameData)
    {
        int score = 0;
        var cnts = CountOverQuantityEveryRank(higher, cnt);
        
        for (int i = 0; i < cnts.Length; i++)
        {
            if (cnts[i] == 0) continue;

            var s = cnts[i] - cnt - gameData.PlayingNumber + gameData.PlayerNumber;
            int n;
            
            if (s <= 0) n = 4;
            else if (s == 1) n = 9;
            else if (s == 2) n = 15;
            else n = 24;
            
            score += n;
        }
        
        return 100 - score;
    }

    private int CountRank(ulong bitCard)
    {
        var qpCard = bit.ToQPCard(bitCard);

        return bit.CountBit(qpCard);
    }

    //指定の枚数以上あるランク内で、何枚カードがあるか数える
    private int[] CountOverQuantityEveryRank(ulong higher, int cnt)
    {
        int n = cnt - 1;
        var noless = 16 - (1ul << n);
        var cntMask = BitUtility.M0001 * noless;
        var qpOver = bit.ToQPCard(higher) & cntMask;

        var cnts = new int[13];

        for (int i = 0; i < 13; i++)
        {
            var c = (qpOver >> i * 4) & 15ul;
            
            if (c == 0) cnts[i] = 0;
            else cnts[i] = (int)Math.Log((int)c, 2) + 1;
        }

        return cnts;
    }
}
