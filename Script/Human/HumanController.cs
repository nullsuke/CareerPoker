using UnityEngine;
using UnityEngine.UI;

public class HumanController : PlayerController
{
    [SerializeField] GameObject controllPanel = default;
    [SerializeField] Button playButton = default;
    private HumanCardViewer viewer;
    
    public override void OnTurn(ulong bitFieldCard, ulong usedCard, int playingNumber,
        int playerNumber, bool isRevolutionalizing)
    {
        base.OnTurn(bitFieldCard, usedCard, playingNumber,
            playerNumber, isRevolutionalizing);

        if (EmpahsizeLegalCard() == 0)
        {
            BitSelectedHand = 0;
            OnEndedTurn();
        }
        else
        {
            controllPanel.gameObject.SetActive(true);
            playButton.gameObject.SetActive(false);
        }
    }

    public override void Render()
    {
        viewer.Render(ToCardIDList(BitPlayerCard));
    }

    public override void Clear()
    {
        base.Clear();
        Render();
    }

    public void Play()
    {
        logic.DrawCard(ID, BitSelectedHand);
        EndTurn();
    }

    public void Pass()
    {
        BitSelectedHand = 0;
        viewer.Cancel();
        EndTurn();
    }

    protected override void Awake()
    {
        base.Awake();

        viewer = GetComponent<HumanCardViewer>();

        viewer.SelectedCardChanged += (s, e) =>
        {
            var viewer = (HumanCardViewer)s;

            BitSelectedHand = CardUtility.ToBitCard(viewer.SelectedCard);

            var isLegal = IsLegalBitCard(gameData.BitFieldCard);

            playButton.gameObject.SetActive(isLegal);
        };
    }

    private ulong EmpahsizeLegalCard()
    {
        var legal = GetLegalBitCard(gameData.BitFieldCard);
        viewer.EmphasizeLegalCard(ToCardIDList(legal));
        
        return legal;
    }

    private ulong GetLegalBitCard(ulong bitFieldCard)
    {
        if (bitFieldCard == 0) return BitPlayerCard;

        int cnt = bit.CountBit(bitFieldCard);
        var higher = bit.GetHigherBitCard(BitPlayerCard, bitFieldCard, 
            gameData.IsRevolutionalizing);
        
        if (bit.IsSequence(bitFieldCard, cnt))
        {
            return bit.GetSequence(higher, cnt);
        }
        else
        {
            return bit.GetGroup(higher, cnt);
        }
    }

    private bool IsLegalBitCard(ulong bitFieldCard)
    {
        int cntSelected = bit.CountBit(BitSelectedHand);

        if (bitFieldCard == 0)
        {
            return bit.IsSequence(BitSelectedHand, cntSelected) ||
                bit.IsGroup(BitSelectedHand, cntSelected);
        }
        else
        {
            int cntField = bit.CountBit(bitFieldCard);

            if (cntField != cntSelected) return false;

            if (bit.IsSequence(bitFieldCard, cntField))
            {
                return bit.IsSequence(BitSelectedHand, cntSelected);
            }
            else
            {
                return bit.IsGroup(BitSelectedHand, cntSelected);
            }
        }
    }

    private void EndTurn()
    {
        controllPanel.gameObject.SetActive(false);
        OnEndedTurn();
    }
}
