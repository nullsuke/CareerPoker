using System.Collections.Generic;
using System.Linq;

public class PlayerCard
{
    private static BitUtility bit;
    private readonly ulong bitPlayerCard;
    //手札内のすべての役
    private List<Hand> hands;

    public List<Hand> Hands { get => hands; }

    public ulong BitPlayerCard { get => bitPlayerCard; }

    //あがれる手札かどうか
    public bool CanEndGame
    {
        get
        {
            int cnt = hands.Count;
            int end = hands.Count(h => h.CanEndTurn);

            return cnt == 0 || cnt - end <= 1;
        }
    }

    //手札評価値を取得
    public float Score
    {
        get
        {
            float score = 0f;

            hands.ForEach(h =>
            {
                if (h.Score >= 1 && h.Score <= 30)
                {
                    score += 2;
                }
                else if (h.Score >= 31 && h.Score <= 60)
                {
                    score += 1;
                }
                else if (h.Score >= 91 && h.EnemyHigherRankQuantity > 0)
                {
                    score += -1;
                }
                else if (h.Score >= 91 && h.EnemyHigherRankQuantity == 0)
                {
                    score += -h.Quantity;
                }
            });

            return score;
        }
    }

    public PlayerCard(ulong bitPlayerCard, GameData gameData)
    {
        this.bitPlayerCard = bitPlayerCard;
        bit = BitUtility.Instance;

        var bitHands = GetBitHands(gameData.IsRevolutionalizing);
        hands = CreateHands(bitHands, gameData);
    }

    //合法手をList<Hand>型で取得する
    public List<Hand> GetLegalHands(GameData gameData)
    {
        int cnt = bit.CountBit(gameData.BitFieldCard);

        if (cnt == 0)
        {
            return hands;
        }
        else
        {
            var bitLegalHands = GetBitLegalHands(cnt, gameData.BitFieldCard,
                gameData.IsRevolutionalizing);
            
            return CreateLegalHands(bitLegalHands, gameData);
        }
    }

    //役を出した後の手札を取得する
    public PlayerCard CreateNextPlayerCard(ulong bitHand,
        GameData gameData)
    {
        var nextBitCard = bitPlayerCard ^ bitHand;
        var used = bitHand | gameData.BitUsedCard;
        var gd = new GameData(0, used, gameData.PlayingNumber, gameData.PlayerNumber,
            gameData.IsRevolutionalizing);

        return new PlayerCard(nextBitCard, gd);
    }

    public override string ToString()
    {
        var s = "";
        hands.ForEach(h =>
        {
            s += "[ " + h.ToString() + " ]" + ", ";
        });

        return s.TrimEnd(new char[] { ',', ' ' });
    }

    private List<Hand> CreateHands(List<ulong> bitHands, GameData gameData)
    {
        var hands = new List<Hand>();

        bitHands.ForEach(h =>
        {
            int same = CountSameQuantity(h, bitPlayerCard);
            var hand = new Hand(h, bitPlayerCard, gameData, same);
            hands.Add(hand);
        });

        this.hands = hands;

        SetPriority(gameData);

        return hands;
    }

    private List<Hand> CreateLegalHands(List<ulong> bitHands, GameData gameData)
    {
        var hands = new List<Hand>();

        bitHands.ForEach(h =>
        {
            var hand = new Hand(h, bitPlayerCard, gameData);
            hands.Add(hand);
        });

        return hands;
    }

    //役をList<ulong>型で取得する
    private List<ulong> GetBitHands(bool isRevolutionalizing)
    {
        var bitHands = new List<ulong>();
        var single = GetSingle(bitPlayerCard);
        var noStrongest = bitPlayerCard;
        var other = bitPlayerCard;
        int cnt = bit.CountBit(bitPlayerCard);

        //手札が4枚以下のときは階段役＋1役以下であり上がりきる可能性が高いので最強ランクも
        //階段に含める
        if (cnt > 4)
        {
            if (isRevolutionalizing)
            {
                noStrongest = bitPlayerCard & 0x000ffffffffffff0ul;
            }
            else
            {
                noStrongest = bitPlayerCard & 0x0000fffffffffffful;
            }
        }

        bit.GetSequenceList(noStrongest).ForEach(s =>
        {
            if ((s & single) > 0)
            {
                bitHands.Add(s);
                other ^= s;
            }
        });

        bit.GetGroupList(other).ForEach(g =>
        {
            bitHands.Add(g);
            other ^= g;
        });

        //BitCard型のカードの集合(other)から1枚ずつ取り出し、BitCard型のリストに変換
        var list = bit.ToBitList(other);
        bitHands.AddRange(list);

        return bitHands;
    }

    //合法手をList<ulong>型で取得する
    private List<ulong> GetBitLegalHands(int cnt, ulong bitFieldCard,
        bool isRevolutionalizing)
    {
        List<ulong> bitHands;

        var higher = bit.GetHigherBitCard(bitPlayerCard, bitFieldCard,
            isRevolutionalizing);

        if (bit.IsSequence(bitFieldCard, cnt))
        {
            bitHands = bit.GetSequenceList(higher, cnt);
        }
        else
        {
            bitHands = new List<ulong>();
            var grps = bit.GetGroupList(higher, cnt);
            
            grps.ForEach(h =>
            {
                //場にある枚数と役を構成する枚数が同じなら組み合わせを考える必要はない
                if (bit.CountBit(h) == cnt)
                {
                    bitHands.Add(h);
                }
                else
                {
                    var newGrps = GetConbinations(h, cnt);
                    bitHands.AddRange(newGrps);
                }
            });
        }

        return bitHands;
    }

    //n枚で構成されるグループ役からr枚取り出してできる役をすべて取得
    private List<ulong> GetConbinations(ulong bitHand, int r)
    {
        int n = bit.CountBit(bitHand);
        //BitCard型のカードの集合(bitHand)から1枚ずつ取り出し、BitCard型のリストに変換
        var bitList = bit.ToBitList(bitHand);
        //n枚で構成されるグループ役からr枚取り出してできるすべての組み合わせを取得。
        //組み合わせはビットで表現されている。
        var cnbs = bit.GetConbinations(n, r);

        var newBitHand = 0ul;
        List<ulong> bitHands = new List<ulong>();

        for (int i = 0; i < cnbs.Count; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if ((cnbs[i] & (1 << j)) > 0)
                {
                    newBitHand |= bitList[j];
                }
            }

            bitHands.Add(newBitHand);
            newBitHand = 0;
        }

        return bitHands;
    }

    //単数のカードを取得
    private ulong GetSingle(ulong bitCard)
    {
        var qpSingle = bit.ToQPCard(bitCard) & BitUtility.M0001;

        return bit.ToBitCard(qpSingle, bitCard);
    }

    //手札内にある同じ枚数の役の数を取得
    private int CountSameQuantity(ulong bitHand, ulong bitPlayerCard)
    {
        int cnt = bit.CountBit(bitHand);

        if (bit.IsSequence(bitHand))
        {
            var seqs = bit.GetSequenceList(bitPlayerCard, cnt);

            return seqs.Count();
        }
        else
        {
            var seqs = bit.GetSequenceList(bitPlayerCard);

            seqs.ForEach(s =>
            {
                bitPlayerCard ^= s;
            });
            
            var qpCard = bit.ToQPCard(bitPlayerCard);
            var n = BitUtility.M0001 << (cnt - 1);

            return bit.CountBit(qpCard & n);
        }
    }

    //優先評価値を計算
    private void SetPriority(GameData gameData)
    {
        var canEnd = CanEndGame;
        var rev = hands.Find(h => h.CanRevolutionalize);

        if (rev != null)
        {
            SetPriorityOnRevolution(rev, canEnd, gameData);
        }
        else
        {
            hands.ForEach(h => h.SetPriority(canEnd));
        }
    }

    //革命できる場合の優先評価値を計算
    private void SetPriorityOnRevolution(Hand rev, bool canEnd, GameData gameData)
    {
        var next = CreateNextPlayerCard(rev.BitHand, gameData);

        if (hands.Exists(h => h.Quantity <= 2 && h.Score > 90) || Score < next.Score)
        {
            rev.Priority = 1;

            foreach (var h in hands.Where(h => !h.CanRevolutionalize))
            {
                h.SetPriority(canEnd);
            }
        }
        else
        {
            rev.Priority = 90;

            foreach (var h in hands.Where(h => !h.CanRevolutionalize))
            {
                if (!bit.IsSequence(h.BitHand) && h.CanEndTurn)
                {
                    h.Priority = 91;
                }
                else
                {
                    h.SetPriority(canEnd);
                }
            }
        }
    }
}