using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighScoreScript : MonoBehaviour
{
    public GameObject Score;
    public GameObject Name;
    public GameObject Rank;

    public void SetScore(string name, string score, string rank)
    {
        this.Rank.GetComponent<Text>().text = rank;
        this.Score.GetComponent<Text>().text = score;
        this.Name.GetComponent<Text>().text = name;
    }
}