using System;
using System.Collections.Generic;

public class Bit
{
    private readonly static ulong M = 0x03F566ED27179461;
    private readonly static int[] table;

    static Bit()
    {
        table = new int[64];
        var m = M;

        for (int i = 0; i < 64; i++)
        {
            table[m >> 58] = i;
            m <<= 1;
        }
    }

    //1の数を数える
    public int CountBit(ulong bit)
    {
        bit = (bit & 0x5555555555555555) + (bit >> 1 & 0x5555555555555555);
        bit = (bit & 0x3333333333333333) + (bit >> 2 & 0x3333333333333333);
        bit = (bit & 0x0f0f0f0f0f0f0f0f) + (bit >> 4 & 0x0f0f0f0f0f0f0f0f);
        bit = (bit & 0x00ff00ff00ff00ff) + (bit >> 8 & 0x00ff00ff00ff00ff);
        bit = (bit & 0x0000ffff0000ffff) + (bit >> 16 & 0x0000ffff0000ffff);
        bit = (bit & 0xffffffffffffffff) + (bit >> 32 & 0xffffffffffffffff);

        return (int)bit;
    }

    //最も下位にある1が何桁目なのか取得
    public int BitScanForward(ulong bit)
    {
        if (bit == 0) return 64;

        var d = bit & GetLeastSignificantBit(bit);
        var i = (d * M) >> 58;

        return table[i];
    }

    //最も上位にある1が何桁目なのか取得
    public int BitScanReverse(ulong bit)
    {
        if (bit == 0) return -1;

        var d = bit & GetMostSignificantBit(bit);
        var i = (d * M) >> 58;

        return table[i];
    }

    //n個のものの中からr個のものを選択してできるすべて組み合わせを取得
    public List<int> GetConbinations(int n, int r)
    {
        var bits = new string[n];

        for (var i = 0; i < n; i++)
        {
            if (n - r <= i) bits[i] = "1";
            else bits[i] = "0";
        }

        var combinations = new List<int>();
        int bit = Convert.ToInt32(String.Join("", bits), 2);

        if (n == r)
        {
            combinations.Add(bit);
            return combinations;
        }

        int low;
        int high;

        do
        {
            combinations.Add(bit);

            low = n;

            for (int i = 0; i < n; i++)
            {
                if ((bit & (1 << i)) > 0)
                {
                    low = i;
                    break;
                }
            }

            high = n;

            for (int i = low + 1; i < n; i++)
            {
                if ((bit & (1 << i)) == 0)
                {
                    high = i;
                    bit |= (1 << high);
                    break;
                }
            }

            if (high < n)
            {
                bit &= ~((1 << high) - 1);
                int cnt = high - low - 1;
                bit |= (1 << cnt) - 1;
            }
        } while (high < n);

        return combinations;
    }

    //最も下位にある1だけを残して取得
    protected ulong GetLeastSignificantBit(ulong bit)
    {
        var comp = ~bit + 1;

        return bit & comp;
    }

    //最も上位にある1だけを残して取得
    protected ulong GetMostSignificantBit(ulong bit)
    {
        var b = bit | (bit >> 1);

        b |= b >> 2;
        b |= b >> 4;
        b |= b >> 8;
        b |= b >> 16;
        b |= b >> 32;

        return b ^ (b >> 1);
    }
}
