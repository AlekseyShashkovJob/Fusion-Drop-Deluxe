using UnityEngine;
using View.Button;

namespace View.UI.Game
{
    public class GameplayScreen : UIScreen
    {
        [SerializeField] private CustomButton _home;
        [SerializeField] private CustomButton _pause;
        [SerializeField] private UIScreen _pauseScreen;

        [SerializeField] private Misc.SceneManagment.SceneLoader _sceneLoader;

        private void OnEnable()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortraitUpsideDown = false;

            _home.AddListener(GameCore.GameManager.Instance.BackToMenu);
            _pause.AddListener(PauseGame);
        }

        private void OnDisable()
        {
            Screen.orientation = ScreenOrientation.AutoRotation;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortraitUpsideDown = false;

            _home.RemoveListener(GameCore.GameManager.Instance.BackToMenu);
            _pause.RemoveListener(PauseGame);
        }

        private void PauseGame()
        {
            _pauseScreen.StartScreen();
        }
    }
}