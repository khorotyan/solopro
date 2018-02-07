using System.Collections;
using Quobject.SocketIoClientDotNet.Client;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

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
    [Header("ChallWaitPanel")]
    public GameObject challWaitPanel;
    public Slider cwChallAcceptTimer;
    public Text cwChallAcceptTime;

    [Space(5)]
    [Header("ChallNotPanel")]
    public GameObject challNotPanel;
    public Text cnOppUsername;
    public Button cnAcceptButton;
    public Button cnRejectButton;
    public Slider cnChallAcceptTimer;
    public Text cnChallAcceptTime;

    [Space(5)]
    [Header("ChallStartPanel")]
    public GameObject challStartPanel;
    public Text csPlUsername;
    public Text csOppUsername;
    public Image csPlIcon;
    public Image csOppIcon;
    public Slider csChallAcceptTimer;
    public Text csChallAcceptTime;

    [Space(5)]
    [Header("GamePanel")]
    public GameObject gamePanel;
    public Slider gaGameTimer;
    public Text gaTimeLeft;
    public Text gaPlUsername;
    public Text gaOppUsername;
    public Text gaPlScore;
    public Text gaOppScore;
    public Image gaPlIcon;
    public Image gaOppIcon;
    public Text gaQuestion;
    public Button gaAns1;
    public Button gaAns2;
    public Button gaAns3;
    public Button gaAns4;
    public GameObject gaQNumShower;
    public Text gaQuestionNum;

    [Space(5)]
    [Header("ResultsPanel")]
    public GameObject resultsPanel;
    public GameObject rsWinShower;
    public GameObject rsDrawShower;
    public GameObject rsLoseShower;
    public Button rsAcceptButton;
    public Button rsRejectButton;
    public Slider rsTimer;
    public Text rsTime;

    [Space(5)]
    [Header("Other")]
    public Sprite maleSprite;
    public Sprite femaleSprite;
    public Sprite inpDefaultSp;
    public Sprite inpWrongSp;
    public Sprite inpHighlightSp;
    public GameObject infoBox;
    public Text infoText;
    public Button connButton;
    public Transform connPanel;
    public InputField connInput;

    [System.NonSerialized]
    public string reqURL;
    [System.NonSerialized]
    public List<Quiz> quizes = new List<Quiz>();

    private float infoTime = 0f;
    private bool canShowInfo = false;
    private bool canShowURlPanel = false;
    private string info = "";  

    private void Awake()
    {
        Application.runInBackground = true;

        connButton.onClick.AddListener(delegate { URLConfig(start: true); });
        connInput.onEndEdit.AddListener(delegate { URLConfig(start: false); });

        InitializeQuizes();
    }

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

    // Configure the connection url
    private void URLConfig(bool start)
    {
        if (start)
        {
            connPanel.GetComponent<RectTransform>().DOAnchorPosY(0, 0.5f).SetEase(Ease.OutBack);
        }
        else
        {
            reqURL = connInput.text;
            transform.parent.GetComponent<ChallengeCont>().enabled = true;
            //transform.parent.GetComponent<ChallengeCont>().socket = IO.Socket(reqURL + "/challenges");
            connPanel.gameObject.SetActive(false);
        }
    }

    private void InitializeQuizes()
    {
        string all = Resources.Load("Questions").ToString();

        string[] individuals = all.Split(new string[] { "--q--" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < individuals.Length; i++)
        {
            string[] ind = individuals[i].Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            quizes.Add(new Quiz(ind[1], ind[2], ind[3], ind[4], ind[5], int.Parse(ind[6])));
        }
    }
}

public class Quiz
{
    public string question;
    public string answer1;
    public string answer2;
    public string answer3;
    public string answer4;
    public int ansNum;

    public Quiz(string question, string answer1, string answer2, string answer3, string answer4, int ansNum)
    {
        this.question = question;
        this.answer1 = answer1;
        this.answer2 = answer2;
        this.answer3 = answer3;
        this.answer4 = answer4;
        this.ansNum = ansNum;
    }
}
