using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float timeLimit = 60f;
    public bool isGameOver;

    int score;
    int highScore;
    bool newHigh;
    float timeLeft;
    bool started;
    float overTime;

    GUIStyle titleStyle, subStyle, hudStyle, overStyle, rankStyle;
    Texture2D dim;

    void Awake()
    {
        Instance = this;
        timeLeft = timeLimit;
        Time.timeScale = 0f;
        highScore = PlayerPrefs.GetInt("highScore", 0);

        dim = new Texture2D(1, 1);
        dim.SetPixel(0, 0, new Color(0f, 0f, 0.1f, 0.6f));
        dim.Apply();
    }

    void Update()
    {
        if (!started)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                started = true;
                Time.timeScale = 1f;
            }
            return;
        }

        if (isGameOver)
        {
            if (Input.GetKeyDown(KeyCode.R))
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            GameOver();
        }
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        if (score < 0) score = 0;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        overTime = Time.time;

        if (score > highScore)
        {
            highScore = score;
            newHigh = true;
            PlayerPrefs.SetInt("highScore", highScore);
            PlayerPrefs.Save();
        }
    }

    void MakeStyles()
    {
        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontSize = 70;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = new Color(0.2f, 1f, 1f);

        subStyle = new GUIStyle(titleStyle);
        subStyle.fontSize = 30;
        subStyle.fontStyle = FontStyle.Normal;
        subStyle.normal.textColor = Color.white;

        hudStyle = new GUIStyle(GUI.skin.label);
        hudStyle.fontSize = 30;
        hudStyle.fontStyle = FontStyle.Bold;
        hudStyle.normal.textColor = new Color(1f, 0.92f, 0.2f);

        overStyle = new GUIStyle(titleStyle);
        overStyle.fontSize = 90;

        rankStyle = new GUIStyle(titleStyle);
        rankStyle.fontSize = 45;
    }

    string GetRank()
    {
        if (score >= 40) return "LEGEND!!!";
        if (score >= 25) return "PRO";
        if (score >= 10) return "NOT BAD";
        return "TRY AGAIN";
    }

    void OnGUI()
    {
        if (titleStyle == null) MakeStyles();

        float w = Screen.width;
        float h = Screen.height;

        if (!started)
        {
            GUI.DrawTexture(new Rect(0, 0, w, h), dim);
            GUI.Label(new Rect(0, h / 2 - 100, w, 100), "GAME START", titleStyle);
            GUI.Label(new Rect(0, h / 2, w, 50), "Press SPACE or click to play", subStyle);
            GUI.Label(new Rect(0, h / 2 + 50, w, 50), "Catch coins, avoid bombs. You have " + Mathf.RoundToInt(timeLimit) + " seconds", subStyle);
            return;
        }

        GUI.Label(new Rect(20, 20, 400, 50), "Score: " + score, hudStyle);
        GUI.Label(new Rect(20, 60, 400, 50), "Time: " + Mathf.CeilToInt(timeLeft), hudStyle);

        if (!isGameOver) return;

        float t = Time.time - overTime;

        // dark overlay with a quick flash at the start
        GUI.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.85f, t * 2f));
        GUI.DrawTexture(new Rect(0, 0, w, h), dim);
        GUI.color = Color.white;

        // rainbow color cycling
        overStyle.normal.textColor = new Color(0.5f, 0f, 0.1f); // dark red / maroon

        // zoom in, then pulse, plus a shake that fades out
        float zoom = Mathf.Clamp(t * 3f, 0.05f, 1f);
        float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 7f);
        float s = zoom * pulse;
        float shake = Mathf.Max(0f, 14f * (1f - t));
        Vector2 off = new Vector2(Random.Range(-shake, shake), Random.Range(-shake, shake));

        Matrix4x4 old = GUI.matrix;
        GUIUtility.ScaleAroundPivot(new Vector2(s, s), new Vector2(w / 2f, h / 2f - 100f));
        GUI.Label(new Rect(off.x, h / 2f - 150f + off.y, w, 120), "GAME OVER", overStyle);
        GUI.matrix = old;

        // score counts up
        int shown = Mathf.Min(score, Mathf.FloorToInt(t * Mathf.Max(score, 1) / 1.5f));
        GUI.Label(new Rect(0, h / 2f - 20f, w, 60), "Final Score: " + shown, subStyle);

        // rank and high score appear after the count finishes
        if (t > 1.5f)
        {
            rankStyle.normal.textColor = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.6f + 0.5f, 1f), 0.85f, 1f);
            GUI.Label(new Rect(0, h / 2f + 35f, w, 60), GetRank(), rankStyle);

            if (newHigh)
            {
                if (Mathf.Sin(Time.time * 10f) > 0f)
                {
                    subStyle.normal.textColor = new Color(1f, 0.92f, 0.2f);
                    GUI.Label(new Rect(0, h / 2f + 90f, w, 50), "NEW HIGH SCORE!", subStyle);
                    subStyle.normal.textColor = Color.white;
                }
            }
            else
            {
                GUI.Label(new Rect(0, h / 2f + 90f, w, 50), "High Score: " + highScore, subStyle);
            }

            GUI.Label(new Rect(0, h / 2f + 140f, w, 50), "Press R to play again", subStyle);
        }
    }
}