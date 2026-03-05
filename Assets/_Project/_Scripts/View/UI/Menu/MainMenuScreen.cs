using UnityEngine;
using View.Button;

namespace View.UI.Menu
{
    public class MainMenuScreen : UIScreen
    {
        [SerializeField] private Misc.SceneManagment.SceneLoader _sceneLoader;

        [SerializeField] private UIScreen _settingsScreen;
        [SerializeField] private UIScreen _privacyScreen;
        [SerializeField] private UIScreen _leaderboardScreen;

        [SerializeField] private CustomButton _startGame;
        [SerializeField] private CustomButton _privacy;
        [SerializeField] private CustomButton _settings;
        [SerializeField] private CustomButton _leaderboard;

        private void OnEnable()
        {
            _startGame.AddListener(OpenGame);
            _privacy.AddListener(OpenPrivacy);
            _settings.AddListener(OpenSettings);
            _leaderboard.AddListener(OpenLeaderboard);
        }

        private void OnDisable()
        {
            _startGame.RemoveListener(OpenGame);
            _privacy.RemoveListener(OpenPrivacy);
            _settings.RemoveListener(OpenSettings);
            _leaderboard.RemoveListener(OpenLeaderboard);
        }

        private void OpenGame()
        {
            _sceneLoader.ChangeScene(Misc.Data.SceneConstants.GAME_SCENE);
            CloseScreen();
        }

        private void OpenPrivacy()
        {
            _privacyScreen.StartScreen();
        }

        private void OpenSettings()
        {
            _settingsScreen.StartScreen();
        }

        private void OpenLeaderboard()
        {
            _leaderboardScreen.StartScreen();
        }
    }
}