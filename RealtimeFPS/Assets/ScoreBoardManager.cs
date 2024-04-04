using Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBoardManager : MonoBehaviour
{
    string occupier = "-";
    Dictionary<string, int> currentScore = new Dictionary<string, int>();

    ScoreBoard scoreBoard;

    void Start()
    {
        scoreBoard = GameUIManager.Instance.FetchPanel<ScoreBoard>();

        NetworkManager.Instance.Client.packetHandler.AddHandler(OnItemOccupied);
        NetworkManager.Instance.Client.packetHandler.AddHandler(OnScored);

        foreach (var client in NetworkManager.Instance.Client.ClientList)
            currentScore.Add(client, 0);

        scoreBoard.SetScoreSize(currentScore.Count);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            scoreBoard.SetOccupier(occupier);
            scoreBoard.SetScore(currentScore);
            GameUIManager.Instance.OpenPanel<ScoreBoard>(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            GameUIManager.Instance.ClosePanel<ScoreBoard>(true);
        }
    }

    public void OnItemOccupied( S_FPS_ITEM_OCCUPIED pkt)
    {
        occupier = pkt.Occupier;
        scoreBoard.SetOccupier(occupier);
    }

    public void OnScored( S_FPS_SCORED pkt)
    {
        if (currentScore.ContainsKey(pkt.Scorer))
            currentScore[pkt.Scorer] += 1;

        scoreBoard.SetScore(currentScore);

        occupier = "-";
        scoreBoard.SetOccupier(occupier);
    }
}
