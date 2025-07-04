using UnityEngine;
using UnityEngine.UI;

public class ResolutionManager : MonoBehaviour
{
    [SerializeField] RectTransform configUI;
    [SerializeField] ScrollRect mazeConfigUI;

    //  Desktop Full HD 
    public void SetDesktopResolution()
    {
        Screen.SetResolution(1920, 1080, true);
        mazeConfigUI.elasticity = 0.1f;
        RectTransform deltaSize = mazeConfigUI.gameObject.GetComponent<RectTransform>();
        deltaSize.sizeDelta = new Vector2(640f, 1080f);
        configUI.sizeDelta = new Vector2(640f, 1080f);

    }

    //  iPad 
    public void SetiPadResolution()
    {
        Screen.SetResolution(2048, 1536, true);
        mazeConfigUI.elasticity = 0.1f;
        RectTransform deltaSize = mazeConfigUI.gameObject.GetComponent<RectTransform>();
        deltaSize.sizeDelta = new Vector2(640f, 1536f);
        configUI.sizeDelta = new Vector2(640f, 1536f);


    }

    //  iPhoneX 
    public void SetiPhoneXResolution()
    {
        Screen.SetResolution(2436, 1125, true);
        mazeConfigUI.elasticity = 3f;
        RectTransform deltaSize = mazeConfigUI.gameObject.GetComponent<RectTransform>();
        deltaSize.sizeDelta = new Vector2(640f, 1080f);
        configUI.sizeDelta = new Vector2(640f, 1080f);

    }
}
