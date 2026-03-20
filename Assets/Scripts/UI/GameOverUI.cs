using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelAchievedText;
    [SerializeField] private Button mainMenuButton;



    void Awake()
    {
        mainMenuButton.onClick.AddListener(() => {
            Loader.Load(Loader.Scene.MainMenuScene);
        });
    }

    private void Start() {
        GameDirector.Instance.OnStateChanged += GameDirector_OnStateChanged;

        Hide();
    }

    private void GameDirector_OnStateChanged(object sender, System.EventArgs e) 
    {
        if (GameDirector.Instance.IsGameOver()) 
        {
            Show();
        
            int levels = GameDirector.Instance.GetLevelsCompleted();
            levelAchievedText.text = levels.ToString();
        } 
        else 
        {
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
