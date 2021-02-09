public class GameData
{
    private readonly ulong bitFieldCard;
    private readonly ulong bitUsedCard;
    private readonly int playingNumber;
    private readonly int playerNumber;
    private readonly bool isRevolutionalizing;

    public GameData(ulong bitFieldCard, ulong bitUsedCard,
         int playingNumber, int playerNumber, bool isRevolutionalizing)
    {
        this.playingNumber = playingNumber;
        this.playerNumber = playerNumber;
        this.bitFieldCard = bitFieldCard;
        this.bitUsedCard = bitUsedCard;
        this.isRevolutionalizing = isRevolutionalizing;
    }

    public ulong BitFieldCard { get => bitFieldCard; }
    public ulong BitUsedCard { get => bitUsedCard; }
    public int PlayingNumber { get => playingNumber; }
    public int PlayerNumber { get => playerNumber; }
    public bool IsRevolutionalizing { get => isRevolutionalizing; }
}
