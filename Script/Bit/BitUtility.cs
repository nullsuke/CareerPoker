using System.Collections.Generic;

public class BitUtility : Bit
{
    public readonly static ulong M0001 = 0x1111111111111111;
    private readonly static ulong M0011 = 0x3333333333333333;
    private readonly static ulong M0100 = 0x4444444444444444;
    private readonly static ulong M0101 = 0x5555555555555555;
    private readonly static ulong M1000 = 0x8888888888888888;
    private readonly static ulong M1111 = 0xffffffffffffffff;

    public static BitUtility Instance { get; } = new BitUtility();

    //階段かどうか判定する。
    public bool IsSequence(ulong bitCard, int cnt)
    {
        if (bitCard == 0 || cnt < 3) return false;

        var head = bitCard;

        for (int i = 0; i < cnt - 1; i++)
        {
            head &= head >> 4;
        }

        return CountBit(head) == 1;
    }

    public bool IsSequence(ulong bitCard)
    {
        int cnt = CountBit(bitCard);

        return IsSequence(bitCard, cnt);
    }

    //グループかどうか判定する
    public bool IsGroup(ulong bitCard, int cnt)
    {
        if (cnt > 4) return false;

        var n = (1ul << cnt - 1) * M0001;
        var qpCard = ToQPCard(bitCard);

        return (qpCard & n) > 0;
    }

    //長さが最大になる組み合わせですべての階段をList<ulong>で取得
    public List<ulong> GetSequenceList(ulong bitCard)
    {
        var sequences = new List<ulong>();
        ulong seq;

        do
        {
            seq = GetLongestSequence(bitCard);

            if (seq != 0)
            {
                sequences.Add(seq);
                bitCard ^= seq;
            }

        } while (seq != 0);

        return sequences;
    }

    //指定した長さの階段をすべてList<ulong>で取得
    public List<ulong> GetSequenceList(ulong bitCard, int cnt)
    {
        var sequences = new List<ulong>();
        var head = bitCard;

        for (int i = 1; i < cnt; i++)
        {
            head &= bitCard >> (i * 4);
        }

        while (head != 0)
        {
            var b = GetLeastSignificantBit(head);
            sequences.Add(CreateSequence(b, cnt));
            head ^= b;
        }

        return sequences;
    }

    //指定した長さ以上の階段をすべて取得
    public ulong GetSequence(ulong bitCard, int cnt)
    {
        var head = bitCard;

        for (int i = 1; i < cnt; i++)
        {
            head &= bitCard >> (i * 4);
        }

        if (head == 0) return 0;

        return CreateSequence(head, cnt);
    }

    //すべてのグループをリストで取得
    public List<ulong> GetGroupList(ulong bitCard)
    {
        return GetGroupList(bitCard, 2);
    }

    //指定した枚数以上のグループをすべてList<ulong>で取得
    public List<ulong> GetGroupList(ulong bitCard, int cnt)
    {
        var groups = new List<ulong>();
        var qpGroup = GetGroupByQPCard(bitCard, cnt);

        ToBitList(qpGroup).ForEach(q =>
        {
            groups.Add(ToBitCard(q, bitCard));
        });

        return groups;
    }

    //指定した枚数以上のグループをすべて取得
    public ulong GetGroup(ulong bitCard, int cnt)
    {
        var qpGroup = GetGroupByQPCard(bitCard, cnt);

        return ToBitCard(qpGroup, bitCard);
    }

    //基準よりランクの高いものを取得
    public ulong GetHigherBitCard(ulong bitCard, ulong basisCard,
        bool isRevolutionzlizing)
    {
        if (!isRevolutionzlizing)
        {
            int n = BitScanReverse(basisCard);
            int r = n < 0 ? 0 : n / 4 + 1;
            var noless = M1111 - ((1ul << r * 4) - 1ul);

            return bitCard & noless;
        }
        else
        {
            int n = BitScanForward(basisCard);
            int r = n < 0 ? 0 : n / 4;
            var nomore = (1ul << r * 4) - 1ul;

            return bitCard & nomore;
        }
    }

    //BitCard型を枚数位置型(QuantityPosition)に変換
    public ulong ToQPCard(ulong bitCard)
    {
        //2ビットごとの1の数を2進数で表現
        var a = ((bitCard & M0101) + ((bitCard >> 1) & M0101));
        //4枚あるランク
        var n4 = a & (a << 2) & M1000;
        //3枚あるランク
        var n3 = (a << 2) & (a >> 1) & M0100;
        n3 |= a & (a << 1) & M0100;
        //4ビットごとの1の数を2進数で表現(4は除く)
        var n12 = ((a & M0011) + ((a >> 2) & M0011)) & M0011;

        if (n3 != 0)
        {
            n4 |= n3;
            //3枚である場合を除く。
            n4 |= n12 & ~(n3 >> 1 | n3 >> 2);
        }
        else
        {
            n4 |= n12;
        }

        return n4;
    }

    //枚数位置型をBitCard型に変換
    public ulong ToBitCard(ulong qpCard, ulong bitCard)
    {
        var result = 0ul;

        while (qpCard != 0)
        {
            int i = BitScanForward(qpCard);
            int r = i / 4;

            result |= bitCard & (15ul << (r * 4));
            qpCard &= qpCard - 1;
        }

        return result;
    }

    //ulongから1ビットずつ取り出してリストへ変換
    public List<ulong> ToBitList(ulong bit)
    {
        var list = new List<ulong>();

        while (bit != 0)
        {
            var b = GetMostSignificantBit(bit);
            list.Add(b);
            bit ^= b;
        }

        return list;
    }

    //一番長い階段を取得
    private ulong GetLongestSequence(ulong bitCard)
    {
        var head = bitCard;
        var max = 0ul;
        int i = 1;

        while (head != 0)
        {
            if (i >= 3) max = head;
            head &= bitCard >> (i * 4);
            i++;
        }

        if (max == 0) return 0;
        else
        {
            var b = GetLeastSignificantBit(max);

            return CreateSequence(b, i - 1);
        }
    }

    //階段を生成
    private ulong CreateSequence(ulong head, int cnt = 3)
    {
        var seq = head;

        for (int i = 1; i < cnt; i++)
        {
            head <<= 4;
            seq |= head;
        }

        return seq;
    }

    //指定した枚数以上のグループを全て枚数位置型で取得
    private ulong GetGroupByQPCard(ulong bitCard, int cnt)
    {
        var qpCard = ToQPCard(bitCard);

        int n = cnt - 1;
        var noless = 16 - (1ul << n);
        var cntMask = M0001 * noless;

        return qpCard & cntMask;
    }

    private BitUtility() { }
}
