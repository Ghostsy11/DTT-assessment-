using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UISettings
{

    public class GameManagerUI : MonoBehaviour
    {
        #region Fields
        [Header("Scripts refreances most be manually linked")]
        [SerializeField] ScreenEffects screenEffects;
        [SerializeField] ExcationMazeOrder mazeOrder;
        [SerializeField] DialogText dialogText;

        public static GameManagerUI instance;

        [Header("Panels settings")]
        [SerializeField] GameObject mainMenu;
        [SerializeField] GameObject loadScreenFadeEffect;
        [SerializeField] GameObject uiConfig;
        [SerializeField] GameObject dialogpanel;




        private bool uiConfigIsOnOff;

        #endregion


        #region Event Functions
        private void Start()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

            }

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleUIConfig();
            }
        }

        public void LoadScene(int sceneIndex)
        {

            mazeOrder.ResetUI();
            StartCoroutine(SceneFadeEffect(screenEffects, loadScreenFadeEffect, sceneIndex));
        }


        public void TurnOffSelectingPanel(GameObject menu)
        {
            if (menu != null)
            {
                menu.SetActive(false);
            }
        }
        public void TurnOnSelectingPanel(GameObject menu)
        {
            if (menu != null)
            {
                menu.SetActive(true);
            }
        }

        public void Quit()
        {
            Application.Quit();
        }



        IEnumerator SceneFadeEffect(ScreenEffects screenEffect, GameObject screenPanelEffect, int sceneIndex)
        {
            TurnOffSelectingPanel(mainMenu);
            TurnOnSelectingPanel(screenPanelEffect);
            screenEffects.PlayFadeScreenEffect();
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(sceneIndex);
            TurnOnSelectingPanel(uiConfig);
            TurnOnSelectingPanel(dialogpanel);
            dialogText.PlayOnStart();
            TurnOffSelectingPanel(screenPanelEffect);
        }


        private void ToggleUIConfig()
        {
            if (!uiConfigIsOnOff)
            {
                UIConfigIsOn();
            }
            else
            {
                UIConfigIsOff();
            }
        }


        private void UIConfigIsOn()
        {
            uiConfigIsOnOff = true;
            uiConfig.SetActive(true);
        }

        private void UIConfigIsOff()
        {
            uiConfigIsOnOff = false;
            uiConfig.SetActive(false);
        }


        #endregion
    }

}