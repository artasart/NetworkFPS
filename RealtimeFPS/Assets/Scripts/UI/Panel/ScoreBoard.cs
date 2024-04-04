using Protocol;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoard : Panel_Base
{
    private TMP_Text occupier;
    private VerticalLayoutGroup scores;

    protected override void Awake()
    {
        base.Awake();

        occupier = transform.Search("ScoreBoard_Occupier").GetComponent<TMP_Text>();
        scores = transform.Search("ScoreBoard_Scores").GetComponent<VerticalLayoutGroup>();
    }

    public void SetOccupier(string occupierId)
    {
        occupier.text = occupierId;
    }
    
    public void SetScoreSize(int size)
    {
        for(int i = 0; i < size; i++)
            Instantiate(Resources.Load<GameObject>(Define.PATH_UI + "Score"), scores.transform);
    }

    public void SetScore(Dictionary<string, int> _scores)
    {
        var sortedScores = new List<KeyValuePair<string, int>>(_scores);
        sortedScores.Sort((x, y) => y.Value.CompareTo(x.Value));

        for(int i = 0; i < sortedScores.Count; i++)
        {
            scores.transform.GetChild(i).Find("Score_ID").GetComponent<TMP_Text>().text = sortedScores[i].Key;
            scores.transform.GetChild(i).Find("Score_Value").GetComponent<TMP_Text>().text = sortedScores[i].Value.ToString();

            if (NetworkManager.Instance.Client.ClientId == sortedScores[i].Key)
            {
                scores.transform.GetChild(i).Find("Score_ID").GetComponent<TMP_Text>().color = Color.green;
                scores.transform.GetChild(i).Find("Score_Value").GetComponent<TMP_Text>().color = Color.green;
            }
            else
            {
                scores.transform.GetChild(i).Find("Score_ID").GetComponent<TMP_Text>().color = Color.white;
                scores.transform.GetChild(i).Find("Score_Value").GetComponent<TMP_Text>().color = Color.white;
            }
        }
    }
}
