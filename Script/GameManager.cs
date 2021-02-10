using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] HumanController human = default;
    [SerializeField] ComputerController computer1 = default;
    [SerializeField] ComputerController computer2 = default;
    [SerializeField] ComputerController computer3 = default;
    [SerializeField] FieldController field = default;
    [SerializeField] GameObject gameOverPanel = default;
    [SerializeField] Text revolutionText = default;
    private List<PlayerController> players;
    private IEnumerator coroutineMain;
    private BitUtility bit;
    private bool isOver;
    private bool isTurnEnd;
    private bool isRevolutionalizing;
    private int passCount;
    private int playingNumber;

    public void Restart()
    {
        gameOverPanel.gameObject.SetActive(false);
        field.Clear();
        players.ForEach(p => p.Clear());

        Start();
    }

    public void BackToTitle()
    {
        SceneManager.LoadScene("Title");
    }

    private void Start()
    {
        players = new List<PlayerController>();
        bit = BitUtility.Instance;
        isOver = false;
        isTurnEnd = false;
        passCount = 0;

        human.Name = "Player";
        human.TurnEnded += (s, e) => isTurnEnd = true;

        computer1.ID = 1;
        computer1.Name = "Enemy1";
        computer1.TurnEnded += (s, e) => isTurnEnd = true;

        computer2.ID = 2;
        computer2.Name = "Enemy2";
        computer2.TurnEnded += (s, e) => isTurnEnd = true;

        computer3.ID = 3;
        computer3.Name = "Enemy3";
        computer3.TurnEnded += (s, e) => isTurnEnd = true;

        players.Add(human);
        players.Add(computer1);
        players.Add(computer2);
        players.Add(computer3);

        DealCards(players);

        playingNumber = players.Count;

        players.ForEach(p => p.Render());

        coroutineMain = CoroutineMain();
        StartCoroutine(coroutineMain);
    }

    private IEnumerator CoroutineMain()
    {
        int idx = 0;
        int playerNumber = players.Count;

        while(!isOver)
        {
            var current = players[idx % playerNumber];
            idx++;

            if (current.IsOver) continue;

            current.OnTurn(field.BitFieldCard, field.BitUsedCard, playingNumber,
                playerNumber, isRevolutionalizing);
            current.ChangePanelColor();

            yield return new WaitWhile(() => !isTurnEnd);

            if (current.BitSelectedHand != 0)
            {
                if (IsRevolutionableHand(current.BitSelectedHand))
                {
                    isRevolutionalizing = !isRevolutionalizing;
                }

                current.Render();
                field.AddCard(current.BitSelectedHand);
                revolutionText.gameObject.SetActive(isRevolutionalizing);
                passCount = 0;

                if (current.IsOver)
                {
                    playingNumber--;
                    passCount = -1;

                    RenderResult(current, playingNumber, playerNumber);

                    if (playingNumber == 1)
                    {
                        var last = players.Find(p => !p.IsOver);
                        last.Open();
                        RenderResult(last, playingNumber - 1, playerNumber);
                        isOver = true;
                    }
                }
            }
            else
            {
                current.RenderText("pass");
                passCount++;
                
                if (passCount == playingNumber - 1)
                {
                    field.Clear();
                    passCount = 0;
                }
            }

            isTurnEnd = false;

            yield return new WaitForSeconds(1.0f);

            current.RestorePanelColor();
        }
        
        gameOverPanel.gameObject.SetActive(true);
    }

    private void DealCards(List<PlayerController> players)
    {
        var ids = new List<int>();

        for (int i = 0; i < 52; i++)
        {
            ids.Add(i);
        }

        for (int i = 0; i < 13; i++)
        {
            for (int j = 0; j < players.Count; j++)
            {
                var r = Random.Range(0, ids.Count);
                var c = ids[r];
                ids.RemoveAt(r);

                players[j].AddCard(c);
            }
        }
    }

    private void RenderResult(PlayerController player, int playingNumber, int playerNumber)
    {
        int winner = playerNumber - playingNumber;

        if (winner == 1) player.RenderText("grich");
        else if (winner == 2) player.RenderText("rich");
        else if (winner > 2 && playingNumber == 1) player.RenderText("poor");
        else if (winner > 2 && playingNumber == 0) player.RenderText("gpoor");
    }

    private bool IsRevolutionableHand(ulong bitCard)
    {
        var isOver3 = bit.IsGroup(bitCard, 4);

        int cnt = bit.CountBit(bitCard);
        var isOver4 = cnt >= 5 && bit.IsSequence(bitCard, cnt);

        return isOver3 || isOver4;
    }
}
