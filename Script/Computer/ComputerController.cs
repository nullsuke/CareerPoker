public class ComputerController : PlayerController
{
    private PlayerCardViewer viewer;

    public override void OnTurn(ulong fieldCard, ulong usedCard, int playingMember,
        int playerMember, bool isRevolutionalizing)
    {
        base.OnTurn(fieldCard, usedCard, playingMember,
            playerMember, isRevolutionalizing);

        BitSelectedHand = logic.SelectBitHand(ID, gameData);
        
        if (BitSelectedHand != 0) logic.DrawCard(ID, BitSelectedHand);

        OnEndedTurn();
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

    public override void Open()
    {
        viewer.Open();
    }

    protected override void Awake()
    {
        base.Awake();
        viewer = GetComponent<PlayerCardViewer>();
    }
}
