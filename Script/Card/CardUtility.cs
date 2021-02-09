using System.Collections.Generic;
using System.Linq;

public class CardUtility
{
    private static readonly Dictionary<int, int> ranks = new Dictionary<int, int>
    {
        {0, 3 }, {1, 4 }, {2, 5 }, {3, 6 }, {4, 7}, {5, 8}, {6, 9}, {7, 10},
        {8, 11 }, {9, 12}, {10, 13}, {11, 1}, {12, 2}
    };

    public static int GetNumber(int rank)
    {
        var pair = ranks.FirstOrDefault(i => i.Key == rank);

        return pair.Value;
    }

    public static int GetRank(int number)
    {
        var pair = ranks.FirstOrDefault(i => i.Value == number);

        return pair.Key;
    }

    public static int GetDigit(Card.SUIT suit, int number)
    {
        int r = GetRank(number);
        int s = (int)suit;

        return r * 4 + s;
    }

    public static ulong ToBitCard(Card.SUIT suit, int number)
    {
        int d = GetDigit(suit, number);
        ulong bitCard = 1ul << d;

        return bitCard;
    }

    public static ulong ToBitCard(List<Card> cards)
    {
        var bitCard = 0ul;

        cards.ForEach(c =>
        {
            bitCard |= 1ul << c.ID;
        });

        return bitCard;
    }

    public static string ToCardString(int digit)
    {
        int r = digit / 4;
        int n = GetNumber(r);
        Card.SUIT s = (Card.SUIT)(digit % 4);

        return s.ToString() + " " + n.ToString();
    }

    public static string ToCardString(ulong bitCard)
    {
        string s = "";
        var bcu = BitUtility.Instance;

        while (bitCard != 0)
        {
            int i = bcu.BitScanForward(bitCard);
            s += CardUtility.ToCardString(i) + ", ";
            bitCard ^= 1ul << i;
        }

        return s.TrimEnd(new char[] { ',', ' ' });
    }
}
