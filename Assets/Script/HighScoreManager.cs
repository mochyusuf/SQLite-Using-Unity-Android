using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Data;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class HighScoreManager : MonoBehaviour
{
    public string connectionString;

    public List<HighScore> highScores = new List<HighScore>();

    public GameObject scorePrefab;
    public Transform ParentScore;

    public int topRanks;

    public int saveScore;

    public InputField enterName;
    public InputField enterBoard;
    public InputField enterType;

    public GameObject nameDialog;

    private string connection;
    private IDbConnection dbcon;
    private IDbCommand dbcmd;
    private IDataReader reader;
    private StringBuilder builder;

    // Use this for initialization
    private void Start()
    {
        //connectionString = "URI=file:" + Application.streamingAssetsPath + "/HighScoreDB.sqlite";
        OpenDB("HighScoreDB");
        //InsertScore("Mochamad", 4000);
        //DeleteScore(3);
        Create_Table();
        DeleteExtraScore();
        Showscores();
    }

    public void OpenDB(string p)
    {
        Debug.Log("Call to OpenDB:" + p);
        // check if file exists in Application.persistentDataPath
        string filepath = Application.persistentDataPath + "/" + p;
        if (!File.Exists(filepath))
        {
            Debug.LogWarning("File \"" + filepath + "\" does not exist. Attempting to create from \"" +
                             Application.dataPath + "!/assets/" + p + ".sqlite");
            // if it doesn't ->
            // open StreamingAssets directory and load the db ->
            WWW loadDB = new WWW("jar:file://" + Application.dataPath + "!/assets/" + p + ".sqlite");
            while (!loadDB.isDone) { }
            // then save to Application.persistentDataPath
            File.WriteAllBytes(filepath, loadDB.bytes);
        }

        //open db connection
        connectionString = "URI=file:" + filepath + ".sqlite";
        Debug.Log("Stablishing connection to: " + connectionString);
        //dbcon = new SqliteConnection(connectionString);
        //dbcon.Open();
    }

    // Table Names
    private string Table_Name = "HighScore";

    private string Player_ID = "PLAYER_ID";
    private string NAME = "NAME";
    private string SCORE = "SCORE";
    private string DATE = "DATE";
    private string Type = "Type";
    private string Board = "Board";

    public void Create_Table()
    {
        string CREATE_TABLE_TODO = "CREATE TABLE if not exists "
            + Table_Name + "(" + Player_ID + " INTEGER PRIMARY KEY AUTOINCREMENT," +
            NAME + " TEXT," +
            SCORE + " INTEGER," +
            DATE + " DATETIME DEFAULT CURRENT_DATE," +
            Type + " TEXT," +
            Board + " INTEGER" + ")";
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                dbCmd.CommandText = CREATE_TABLE_TODO;

                dbCmd.ExecuteScalar();
                dbConnection.Close();
            }
        }
    }

    public void EnterName()
    {
        if (enterName.text != string.Empty)
        {
            int score = UnityEngine.Random.Range(4000, 5000);
            print(enterName.text + "||" + score);
            InsertScore(enterName.text, int.Parse(enterBoard.text), enterType.text, score);
            enterName.text = string.Empty;

            DeleteExtraScore();
            Showscores();
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            nameDialog.SetActive(!nameDialog.activeSelf);
        }
    }

    private void InsertScore(string name, int Board, string Type, int newScore)
    {
        GetScores();
        int hsCount = highScores.Count;

        if (highScores.Count > 0)
        {
            HighScore lowestScore = highScores[highScores.Count - 1];
            if (lowestScore != null && saveScore > 0 && highScores.Count >= saveScore && newScore > lowestScore.Score)
            {
                DeleteScore(lowestScore.ID);
                hsCount--;
            }
        }
        if (hsCount < saveScore)
        {
            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    string sqlQuery = String.Format("INSERT INTO HighScore(Name,Score) VALUES (\"{0}\",\"{1}\")", name, newScore);

                    dbCmd.CommandText = sqlQuery;

                    dbCmd.ExecuteScalar();
                    dbConnection.Close();
                }
            }
        }
    }

    private void GetScores()
    {
        highScores.Clear();

        foreach (GameObject score in GameObject.FindGameObjectsWithTag("Score"))
        {
            Destroy(score);
        }
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * From HighScore";

                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        highScores.Add(new HighScore(reader.GetInt32(0), reader.GetInt32(2), reader.GetString(1), reader.GetDateTime(3)));
                    }

                    dbConnection.Close();
                    reader.Close();
                }
            }
        }

        highScores.Sort();
    }

    private void DeleteScore(int ID)
    {
        using (IDbConnection dbConnection = new SqliteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = String.Format("DELETE FROM HighScore WHERE PLAYERID = (\"{0}\")", ID);

                dbCmd.CommandText = sqlQuery;

                dbCmd.ExecuteScalar();
                dbConnection.Close();
            }
        }
    }

    private void Showscores()
    {
        for (int i = 0; i < topRanks; i++)
        {
            if (i <= highScores.Count - 1)
            {
                GameObject tempObject = Instantiate(scorePrefab);

                HighScore tempScore = highScores[i];

                tempObject.GetComponent<HighScoreScript>().SetScore(tempScore.Name, tempScore.Score.ToString(), (i + 1).ToString());
                tempObject.transform.parent = ParentScore;
                tempObject.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
            }
        }
    }

    private void DeleteExtraScore()
    {
        GetScores();

        if (saveScore <= highScores.Count)
        {
            int deleteCount = highScores.Count - saveScore;
            //highScores.Reverse();

            using (IDbConnection dbConnection = new SqliteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    for (int i = 0; i < deleteCount; i++)
                    {
                        string sqlQuery = String.Format("DELETE FROM HighScore WHERE PLAYERID = (\"{0}\")", highScores[i].ID);

                        dbCmd.CommandText = sqlQuery;

                        dbCmd.ExecuteScalar();
                    }
                    dbConnection.Close();
                }
            }
        }
    }
}