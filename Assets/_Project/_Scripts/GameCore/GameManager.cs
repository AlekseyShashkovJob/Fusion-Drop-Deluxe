using TMPro;
using UnityEngine;
using System.Collections;

namespace GameCore
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [SerializeField] private OrchardGrid gridManager;
        [SerializeField] private BeltManager beltManager;

        [SerializeField] private View.UI.UIScreen _gameScreen;
        [SerializeField] private View.UI.UIScreen _tutorialScreen;
        [SerializeField] private View.UI.UIScreen _winScreen;
        [SerializeField] private View.UI.UIScreen _loseScreen;
        [SerializeField] private Misc.SceneManagment.SceneLoader _sceneLoader;
        [SerializeField] private TMP_Text _score;

        public int CurrentScore { get; private set; } = 0;
        public int TotalScore { get; private set; } = 0;

        private const int WIN_VALUE = 1024;

        private bool hasWon = false;
        private bool hasLost = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Time.timeScale = 1.0f;

            CurrentScore = 0;
            _score.text = "0";

            gridManager.ClearGrid();
			_tutorialScreen.StartScreen();
        }
		
        public void StartGameFromTutorial()
        {
            _tutorialScreen.CloseScreen();

			beltManager.StartSpawning();
        }

        public void GameOver()
        {
            if (hasLost || hasWon) return;

            hasLost = true;
            Debug.Log("Game Over");
            beltManager.StopSpawning();
            SaveData();
            _loseScreen.StartScreen();
        }

        public void GameWin()
        {
            if (hasWon || hasLost) return;

            hasWon = true;
            Debug.Log("Game Win");
            beltManager.StopSpawning();
            SaveData();
            _winScreen.StartScreen();
        }

        public void BackToMenu()
        {
            Time.timeScale = 1.0f;
            _gameScreen.CloseScreen();
            _sceneLoader.ChangeScene(Misc.Data.SceneConstants.MENU_SCENE);
        }

        public void RestartGame()
        {
            Time.timeScale = 1.0f;
            _gameScreen.CloseScreen();
            _sceneLoader.ChangeScene(Misc.Data.SceneConstants.GAME_SCENE);
        }

        private void SaveData()
        {
            int savedTotal = PlayerPrefs.GetInt(GameConstants.TOTAL_SCORE_KEY, 0);
            if (CurrentScore > savedTotal)
            {
                PlayerPrefs.SetInt(GameConstants.TOTAL_SCORE_KEY, CurrentScore);
                PlayerPrefs.Save();
                TotalScore = CurrentScore;
            }
        }

        private void LoadData()
        {
            TotalScore = PlayerPrefs.GetInt(GameConstants.TOTAL_SCORE_KEY, 0);
        }

        // Метод для добавления очков без проверки победы (во время слияния)
        public void AddScoreNoWinCheck(int value)
        {
            int index = ItemPool.Instance.fruitDatas.FindIndex(f => f.value == value);
            if (index >= 0)
            {
                CurrentScore += index;
                _score.text = CurrentScore.ToString();
            }
        }

        // Проверка победы — вызывается только после всех слияний и анимаций
        public void CheckWinCondition()
        {
            if (hasWon || hasLost) return;

            for (int x = 0; x < gridManager.width; x++)
            {
                for (int y = 0; y < gridManager.height; y++)
                {
                    var fruit = gridManager.GetFruitAt(x, y);
                    if (fruit != null && fruit.Data.value == WIN_VALUE)
                    {
                        GameWin();
                        return;
                    }
                }
            }
        }
    }
}