using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelAchievedText;


    private void Start() {
        GameDirector.Instance.OnStateChanged += GameDirector_OnStateChanged;

        Hide();
    }

    private void GameDirector_OnStateChanged(object sender, System.EventArgs e) {
        if (GameDirector.Instance.IsGameOver()) {
            Show();

            levelAchievedText.text = "Nincsenek még szintek, így nem tudom kiírni a teljesített színtek számát.";
        } else {
            Hide();
        }
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
