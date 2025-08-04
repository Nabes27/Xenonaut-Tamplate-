using UnityEngine;
using TMPro;

public class MenuHighScoreDisplay : MonoBehaviour
{
    public TMP_Text highScoreText;

    void Start()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
    }

    public void ResetHighScore()
    {
        PlayerPrefs.DeleteKey("HighScore");
        PlayerPrefs.Save();

        if (highScoreText != null)
            highScoreText.text = "High Score: 0";
    }
}
