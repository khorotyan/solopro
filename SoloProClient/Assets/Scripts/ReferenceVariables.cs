using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ReferenceVariables : MonoBehaviour
{
    [Header("Signup Panel")]
    public GameObject signupPanel;
    public InputField suUsernameInput;
    public InputField suEmailInput;
    public InputField suPasswordInput;
    public InputField suPassConfInput;
    public Toggle suMaleToggle;
    public Toggle suFemaleToggle;
    public Toggle suAutoLoginToggle;
    public Button suCrAccountButton;
    public Button suSigninPrButton;

    [Space(5)]
    [Header("Signin Panel")]
    public GameObject signinPanel;
    public InputField siEmailInput;
    public InputField siPasswordInput;
    public Toggle siAutoLoginToggle;
    public Button siLoginButton;
    public Button siSignupPrButton;

    [Space(5)]
    [Header("Main Page")]
    public GameObject mainPage;
    public Image maUserIcon;
    public Text maUsernameText;
    public Text maNoWinsText;
    public Button maPlayButton;
    public Button maLogoutButton;

    [Space(5)]
    [Header("Notifications Panel")]
    public GameObject notificationsPanel;
    public GameObject notificationsObj;
    public Button ntCloseButton;
    public Text ntNotCountText;
    public Button ntChallengeButton;
    public Transform ntNotsContainer;

    [Space(5)]
    [Header("Challenges Panel")]
    public GameObject challengesPanel;
    public GameObject challengesObj;
    public Button chCloseButton;
    public Transform chChallsContainer;

    [Space(5)]
    [Header("Other")]
    public GameObject playPage;
    public Sprite maleSprite;
    public Sprite femaleSprite;
    public Sprite inpDefaultSp;
    public Sprite inpWrongSp;
    public Sprite inpHighlightSp;
    public GameObject infoBox;
    public Text infoText;

    [System.NonSerialized]
    public string reqURL;

    private float infoTime = 0f;
    private bool canShowInfo = false;
    private string info = "";

    private void Update()
    {
        SendInfo();
    }

    public void ShowInfo(string info)
    {
        this.info = info;
        infoTime = 0;
        canShowInfo = true;
    }

    // Shows user warnings
    private void SendInfo()
    {
        if (canShowInfo == true)
        {
            if (infoTime == 0)
            {
                infoBox.GetComponent<RectTransform>().DOAnchorPosY(-1770f, 0.5f).SetEase(Ease.OutBack);
                infoText.text = info;
            }

            infoTime += Time.deltaTime;

            if (infoTime > 2f)
            {
                infoBox.GetComponent<RectTransform>().DOAnchorPosY(-1920f, 0.5f).SetEase(Ease.OutBack);
                canShowInfo = false;
            }   
        }   
    }
}
