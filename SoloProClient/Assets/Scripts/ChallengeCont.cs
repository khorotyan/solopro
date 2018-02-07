using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Quobject.SocketIoClientDotNet.Client;

public class ChallengeCont : MonoBehaviour
{
    private ReferenceVariables rv;

    public Socket socket;

    private Color32 btnNormCol = new Color32(255, 255, 255, 255);
    private Color32 btnCorrCol = new Color32(195, 255, 193, 255);
    private Color32 btnWrongCol = new Color32(255, 219, 219, 255);

    private string oppUsername = "";

    private bool waitingForOpponent;
    private bool waitingForChallAcc;
    private bool waitTillChallStart;
    private bool canStartQuizTimer;
    private bool canShowResults;
    private bool canStartBothAns;
    private bool challCancelled;

    private float challRefTime;
    private float challWaitTime;
    private float challAccTime;
    private float challStartTime;
    private float quizTime;
    private float resultsTime;
    private float bothAnsTime;

    private int[] sessionQs;
    private int plScore = 0;
    private int oppScore = 0;
    private int round;
    private int noAnswers;
    private bool enMale;

    public void Initialize()
    {
        waitingForOpponent = false;
        waitingForChallAcc = false;
        waitTillChallStart = false;
        canStartQuizTimer = false;
        canShowResults = false;
        canStartBothAns = false;
        challCancelled = false;

        challRefTime = 0;
        challWaitTime = 5;
        challAccTime = 5;
        challStartTime = 5;
        quizTime = 15;
        resultsTime = 5;
        bothAnsTime = 2;

        sessionQs = new int[3];
        round = 0;
        noAnswers = 0;
        enMale = false;
    }

    private void Awake()
    {
        Initialize();
        rv = transform.GetChild(0).GetComponent<ReferenceVariables>();

        rv.maPlayButton.onClick.AddListener(delegate { OnMainPlayClick(); });

        rv.ntCloseButton.onClick.AddListener(delegate { OnNotifPanelClose(); });

        rv.ntChallengeButton.onClick.AddListener(delegate { OnChallengeClick(); });

        rv.chCloseButton.onClick.AddListener(delegate { OnChallPageClose(); });

        rv.gaAns1.onClick.AddListener(delegate { OnAnswerButtonClick(buttonNum: 1); });
        rv.gaAns2.onClick.AddListener(delegate { OnAnswerButtonClick(buttonNum: 2); });
        rv.gaAns3.onClick.AddListener(delegate { OnAnswerButtonClick(buttonNum: 3); });
        rv.gaAns4.onClick.AddListener(delegate { OnAnswerButtonClick(buttonNum: 4); });
    }

    private void OnDestroy()
    {
        DisconnectSocket();
    }

    private void Start()
    {
        socket = IO.Socket(rv.reqURL + "/challenges");

        CheckForChallenge();
        CheckChallAccept();
        GetOpponentScore();
    }
    
    private void Update()
    {
        RefreshChallPlayers();

        WaitForOpponent();
        WaitForChallenge();
        ManageQuizTimer();
        ShowResults();
        ToNextQuestion();

        ChallNotificationManage();
    }

    private void OnMainPlayClick()
    {
        PlayerCont.inChallPanel = true;
        StartCoroutine(LoadChallengePlayers());

        rv.challengesPanel.SetActive(true);
    }

    private void OnNotifPanelClose()
    {
        PlayerCont.inNotPanel = false;

        rv.notificationsPanel.SetActive(false);
    }

    private void OnChallengeClick()
    {
        PlayerCont.inChallPanel = true;
        PlayerCont.inNotPanel = false;

        StartCoroutine(LoadChallengePlayers());

        rv.challengesPanel.SetActive(true);
        rv.notificationsPanel.SetActive(false);
    }

    private void OnChallPageClose()
    {
        PlayerCont.inChallPanel = false;

        rv.challengesPanel.SetActive(false);
    }

    private IEnumerator LoadChallengePlayers()
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json");
        headers.Add("Authorization", "Bearer " + transform.GetComponent<PlayerCont>().token);

        WWW www = new WWW(rv.reqURL + "/users", null, headers);
        yield return www;

        if (www.error == null)
        {
            for (int i = rv.chChallsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(rv.chChallsContainer.GetChild(i).gameObject);
            }

            string jsonData = www.text;

            UsersList sc = (UsersList) JsonConvert.DeserializeObject(jsonData, typeof(UsersList));

            string plUsername = PlayerCont.username;

            for (int i = 0; i < sc.users.Count; i++)
            {
                string oppUsername = sc.users[i].username;

                if (oppUsername != plUsername)
                {
                    Transform challUser = Instantiate(rv.challengesObj, rv.chChallsContainer).transform;

                    bool male = sc.users[i].male;

                    challUser.GetChild(0).GetComponent<Image>().sprite = male == false ? rv.femaleSprite : rv.maleSprite;
                    challUser.GetChild(1).GetComponent<Text>().text = sc.users[i].username;
                    challUser.GetChild(2).GetComponent<Text>().text = "Wins: " + sc.users[i].wins;
        
                    if (sc.users[i].online == true)
                    {
                        challUser.GetChild(4).GetChild(2).gameObject.SetActive(true);
                        challUser.GetChild(4).GetChild(1).gameObject.SetActive(false);

                        challUser.GetChild(3).gameObject.SetActive(true);
                        challUser.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { OnChallengeSend(plUsername, oppUsername, male); });
                    }
                }   
            }
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    private IEnumerator UpdatePlayerScore()
    {
        WWWForm form = new WWWForm();
        form.AddField("email", PlayerCont.email);
        form.AddField("wins", PlayerCont.wins);

        Dictionary<string, string> headers = form.headers;
        headers.Add("Authorization", "Bearer " + transform.GetComponent<PlayerCont>().token);
        
        WWW www = new WWW(rv.reqURL + "/users/wins", form.data, headers);
        yield return www;

        if (www.error == null)
        {
            string jsonData = www.text;        
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    private IEnumerator GetPlayerInfo()
    {
        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json");
        headers.Add("Authorization", "Bearer " + transform.GetComponent<PlayerCont>().token);

        WWW www = new WWW(rv.reqURL + "/users" + PlayerCont.email, null, headers);
        yield return www;

        if (www.error == null)
        {
            string jsonData = www.text;

            User user = JsonUtility.FromJson<User>(jsonData);

            PlayerCont.wins = user.wins;
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    private void RefreshChallPlayers()
    {
        if (PlayerCont.inChallPanel == true)
        {
            challRefTime += Time.deltaTime;

            if (challRefTime > 5)
            {
                StartCoroutine(LoadChallengePlayers());

                challRefTime = 0;
            }
        }    
    }
    
    // Notify the server that there is a challenge (send a challenge) - Challenge Sender
    private void OnChallengeSend(string player, string opponent, bool male)
    {
        Challenge challenge = new Challenge();
        challenge.player = player;
        challenge.opponent = opponent;

        if (PlayerCont.male == true)
        {
            challenge.male = true;
        }
        else
        {
            challenge.male = false;
        }
         
        socket.Emit("CHALLENGE_SENT", JsonUtility.ToJson(challenge));

        if (male == true)
        {
            rv.gaOppIcon.sprite = rv.maleSprite;
            rv.csOppIcon.sprite = rv.maleSprite;
        }
        else
        {
            rv.gaOppIcon.sprite = rv.femaleSprite;
            rv.csOppIcon.sprite = rv.femaleSprite;
        }

        rv.csPlUsername.text = PlayerCont.username.ToString();
        rv.csOppUsername.text = opponent;

        oppUsername = opponent;
        rv.challWaitPanel.SetActive(true);
        rv.challengesPanel.SetActive(false);
        PlayerCont.inChallPanel = false;
        PlayerCont.busy = true;
        waitingForOpponent = true;
    }

    // Check for challenges from the server - Challenge Receiver
    private void CheckForChallenge()
    {
        socket.On("CHALLENGE_SENT_BACK", (data) =>
        {
            if (PlayerCont.online && !PlayerCont.busy)
            {
                Challenge challenge = JsonUtility.FromJson<Challenge>(data.ToString());

                // If a challenge is sent to the player
                if (challenge.player == PlayerCont.username)
                {
                    enMale = challenge.male;

                    oppUsername = challenge.opponent;
                    waitingForChallAcc = true;
                    PlayerCont.inChallPanel = false;
                    PlayerCont.busy = true;
                }
            }
        });
    }

    // Wait for the opponent, untill he/she accepts the challenge - Challenge Sender
    private void WaitForOpponent()
    {
        if (waitingForOpponent == true)
        {
            challWaitTime -= Time.deltaTime;

            if (challWaitTime >= 0)
            {
                rv.cwChallAcceptTimer.value = challWaitTime;
                rv.cwChallAcceptTime.text = challWaitTime.ToString("N1");
            }
            else if (challWaitTime <= -0.2f)
            {
                //  Cancel the challenge
                rv.challengesPanel.SetActive(true);
                rv.challWaitPanel.SetActive(false);
                PlayerCont.inChallPanel = true;
                PlayerCont.busy = false;
                waitingForOpponent = false;
                challWaitTime = 5;
            }
        }  
    }

    // Manage the screen of challange notifications,
    //  when another player sends a request to this player - Challenge Receiver
    private void ChallNotificationManage()
    {
        if (waitingForChallAcc == true)
        {
            if (challAccTime == 5)
            {
                if (enMale == true)
                {
                    rv.gaOppIcon.sprite = rv.maleSprite;
                    rv.csOppIcon.sprite = rv.maleSprite;
                }
                else
                {
                    rv.gaOppIcon.sprite = rv.femaleSprite;
                    rv.csOppIcon.sprite = rv.femaleSprite;
                }

                rv.challNotPanel.SetActive(true);

                rv.cnAcceptButton.onClick.RemoveAllListeners();
                rv.cnRejectButton.onClick.RemoveAllListeners();
                rv.cnAcceptButton.onClick.AddListener(delegate { OnChallengeAccRejSend(accept: true); });
                rv.cnRejectButton.onClick.AddListener(delegate { OnChallengeAccRejSend(accept: false); });
            }

            challAccTime -= Time.deltaTime;

            if (challAccTime >= 0)
            {
                rv.cnChallAcceptTimer.value = challAccTime;
                rv.cnChallAcceptTime.text = challAccTime.ToString("N1");
            }
            else if (challAccTime <= -0.2f)
            {
                //  Cancel the challenge 
                if (rv.challengesPanel.activeSelf == true)
                {
                    PlayerCont.inChallPanel = true;
                }

                rv.challNotPanel.SetActive(false);
                PlayerCont.busy = false;
                waitingForChallAcc = false;
                challAccTime = 5;
            }
        }
    }

    // Send the challenge accept/reject information to the server - Challenge Receiver
    private void OnChallengeAccRejSend(bool accept)
    {
        if (accept == true)
        {
            for (int i = 0; i < 3; i++)
            {
                int qNum = Random.Range(0, rv.quizes.Count);
                sessionQs[i] = qNum;
            }
 
            Challenge challenge = new Challenge();
            challenge.player = PlayerCont.username;
            challenge.opponent = oppUsername;
            challenge.accepted = accept;
            challenge.questions = sessionQs;

            socket.Emit("CHALLENGE_ACCREJ", JsonUtility.ToJson(challenge));

            rv.csPlUsername.text = PlayerCont.username.ToString();
            rv.csOppUsername.text = oppUsername;

            waitTillChallStart = true;
        }
        else
        {
            Challenge challenge = new Challenge();
            challenge.player = PlayerCont.username;
            challenge.opponent = oppUsername;
            challenge.accepted = accept;

            socket.Emit("CHALLENGE_ACCREJ", JsonUtility.ToJson(challenge));

            PlayerCont.busy = false;
        }

        waitingForChallAcc = false;
        rv.challNotPanel.SetActive(false);
        challAccTime = 5;
        challWaitTime = 5;
    }

    // Check whether the opponent accepted the challenge - Challenge Sender
    private void CheckChallAccept()
    {
        socket.On("CHALLENGE_ACCREJ_BACK", (data) =>
        {
            if (PlayerCont.online)
            {
                Challenge challenge = JsonUtility.FromJson<Challenge>(data.ToString());

                // If a challenge is sent to the player
                if (challenge.player == PlayerCont.username && challenge.opponent == oppUsername)
                {          
                    if (challenge.accepted == true)
                    {
                        sessionQs = challenge.questions;

                        waitTillChallStart = true;

                        challAccTime = 5;
                        challWaitTime = 5;
                    }
                    else
                    {
                        challCancelled = true;
                        PlayerCont.inChallPanel = true;
                        PlayerCont.busy = false;
                        waitingForOpponent = false;
                        challWaitTime = 5;
                    }

                    if (challenge.cancelled == true)
                    {
                        PlayerCont.busy = false;
                        challCancelled = true;
                    }
                }
            }
        });
    }

    // Wait untill the challenge starts - both sender and receiver
    private void WaitForChallenge()
    {
        if (waitTillChallStart == true)
        {
            if (challStartTime == 5)
            {
                rv.challStartPanel.SetActive(true);
            }

            challStartTime -= Time.deltaTime;

            if (challStartTime >= 0)
            {
                rv.csChallAcceptTimer.value = challStartTime;
                rv.csChallAcceptTime.text = challStartTime.ToString("N1");
            }
            else if (challStartTime <= -0.2f)
            {
                StartCoroutine(TurnOffLobby());

                StartCoroutine(AnimateChallQuestion());
            }
        }

        if (challCancelled == true)
        {
            rv.challengesPanel.SetActive(true);
            rv.challWaitPanel.SetActive(false);
            rv.challStartPanel.SetActive(false);
            Initialize();

            challCancelled = false;
        }
    }
    
    // Close the lobby
    private IEnumerator TurnOffLobby()
    {
        waitTillChallStart = false;
        challStartTime = 5;

        yield return new WaitForSeconds(0.5f);

        rv.gaPlUsername.text = PlayerCont.username;
        rv.gaOppUsername.text = oppUsername;
        rv.challStartPanel.SetActive(false);
        rv.gamePanel.SetActive(true);  
    }

    // Show question number with an animation
    private IEnumerator AnimateChallQuestion()
    {
        round++;

        rv.gaAns1.interactable = false;
        rv.gaAns2.interactable = false;
        rv.gaAns3.interactable = false;
        rv.gaAns4.interactable = false;

        Color32 imageCol = rv.gaQNumShower.GetComponent<Image>().color;
        imageCol.a = 255;
        rv.gaQNumShower.GetComponent<Image>().DOColor(imageCol, 0.5f);

        rv.gaQuestionNum.GetComponent<Text>().text = "Round " + round;
        Color32 textCol = rv.gaQuestionNum.GetComponent<Text>().color;
        textCol.a = 255;
        rv.gaQuestionNum.GetComponent<Text>().DOColor(textCol, 0.5f);

        yield return new WaitForSeconds(0.5f);

        rv.gaAns1.GetComponent<Image>().color = btnNormCol;
        rv.gaAns2.GetComponent<Image>().color = btnNormCol;
        rv.gaAns3.GetComponent<Image>().color = btnNormCol;
        rv.gaAns4.GetComponent<Image>().color = btnNormCol;
        quizTime = 15;
        noAnswers = 0;
        rv.gaGameTimer.value = 15;
        rv.gaTimeLeft.text = "15";
        LoadQuestion();

        yield return new WaitForSeconds(1f);

        imageCol.a = 0;
        rv.gaQNumShower.GetComponent<Image>().DOColor(imageCol, 0.5f);

        textCol.a = 0;
        rv.gaQuestionNum.GetComponent<Text>().DOColor(textCol, 0.5f);

        yield return new WaitForSeconds(0.5f);

        canStartQuizTimer = true;

        rv.gaAns1.interactable = true;
        rv.gaAns2.interactable = true;
        rv.gaAns3.interactable = true;
        rv.gaAns4.interactable = true;
    }

    // Load challenge ui
    private void LoadQuestion()
    {
        Quiz currQ = rv.quizes[sessionQs[round - 1]];
        rv.gaQuestion.text = currQ.question;
        rv.gaAns1.transform.GetChild(0).GetComponent<Text>().text = currQ.answer1;
        rv.gaAns2.transform.GetChild(0).GetComponent<Text>().text = currQ.answer2;
        rv.gaAns3.transform.GetChild(0).GetComponent<Text>().text = currQ.answer3;
        rv.gaAns4.transform.GetChild(0).GetComponent<Text>().text = currQ.answer4;
    }

    // Manage challenge question timer
    private void ManageQuizTimer()
    {
        if (canStartQuizTimer == true)
        {
            quizTime -= Time.deltaTime;

            if (quizTime >= 0)
            {
                rv.gaGameTimer.value = quizTime;

                if (quizTime > 3)
                {
                    rv.gaTimeLeft.text = quizTime.ToString("N0");
                }
                else
                {
                    rv.gaTimeLeft.text = quizTime.ToString("N1");
                }
            }
            else
            {
                canStartQuizTimer = false;
                quizTime = 15;

                int corrAns = rv.quizes[sessionQs[round - 1]].ansNum;
                
                Button corrButton = GetButtonFromID(corrAns);
                corrButton.GetComponent<Image>().color = btnCorrCol;

                StartCoroutine(OnTimeEnd());         
            }
        }
    }

    // Wait a little and show the correct answer
    private IEnumerator OnTimeEnd()
    {
        yield return new WaitForSeconds(1f);

        if (round <= 2)
        {
            StartCoroutine(AnimateChallQuestion());
        }
        else
        {
            Initialize();
            canShowResults = true;
        }
    }

    // Whenever the user answers the question, check whether correct or not
    private void OnAnswerButtonClick(int buttonNum)
    {
        int corrAns = rv.quizes[sessionQs[round - 1]].ansNum;

        Button selButton = GetButtonFromID(buttonNum);
        Button corrButton = GetButtonFromID(corrAns);

        if (buttonNum == corrAns)
        {
            plScore += (15 + (int) rv.gaGameTimer.value);
            selButton.GetComponent<Image>().color = btnCorrCol;
        }
        else
        {
            plScore -= 5;
            selButton.GetComponent<Image>().color = btnWrongCol;

            StartCoroutine(showCorrAns(corrButton));
        }

        rv.gaPlScore.text = plScore.ToString();

        rv.gaAns1.interactable = false;
        rv.gaAns2.interactable = false;
        rv.gaAns3.interactable = false;
        rv.gaAns4.interactable = false;

        Challenge challenge = new Challenge();
        challenge.player = PlayerCont.username;
        challenge.opponent = oppUsername;
        challenge.points = plScore;

        noAnswers++;
        socket.Emit("ANSWER_SENT", JsonUtility.ToJson(challenge));
    }

    private void GetOpponentScore()
    {
        socket.On("ANSWER_SENT_BACK", (data) =>
        {
            if (PlayerCont.online)
            {
                Challenge challenge = JsonUtility.FromJson<Challenge>(data.ToString());

                // If a challenge is sent to the player
                if ((challenge.player == PlayerCont.username && challenge.opponent == oppUsername) ||
                    (challenge.opponent == PlayerCont.username && challenge.player == oppUsername))
                {
                    oppScore = challenge.points;
                    noAnswers++;
                }
            }
        });
    }

    // When both of the players answer, go to the next question
    private void ToNextQuestion()
    {
        if (noAnswers == 2 && rv.gaGameTimer.value > 2 && canStartBothAns == false)
        {
            canStartBothAns = true;     
        }

        rv.gaOppScore.text = oppScore.ToString();

        if (canStartBothAns == true)
        {
            bothAnsTime -= Time.deltaTime;

            if (bothAnsTime <= 0)
            {
                canStartQuizTimer = false;
                quizTime = 15;
                bothAnsTime = 2;
                noAnswers = 0;
                canStartBothAns = false;

                if (round <= 2)
                {
                    StartCoroutine(AnimateChallQuestion());
                }
                else
                {
                    Initialize();
                    canShowResults = true;
                }
            }
        }
    }

    // Shows the correct answer of the question
    private IEnumerator showCorrAns(Button corrButton)
    {
        yield return new WaitForSeconds(1f);

        corrButton.GetComponent<Image>().color = btnCorrCol;
    }

    private Button GetButtonFromID(int id)
    {
        if (id == 1)
            return rv.gaAns1;
        else if (id == 2)
            return rv.gaAns2;
        else if (id == 3)
            return rv.gaAns3;
        else
            return rv.gaAns4;
    }

    private void ShowResults()
    {
        if (canShowResults == true)
        {
            if (resultsTime == 5)
            {
                if (plScore > oppScore)
                {
                    PlayerCont.wins++;
                    rv.maNoWinsText.text = "Wins: " + PlayerCont.wins;
                    rv.rsWinShower.SetActive(true);
                    rv.rsDrawShower.SetActive(false);
                    rv.rsLoseShower.SetActive(false);
                }
                else if (plScore < oppScore)
                {
                    rv.rsLoseShower.SetActive(true);
                    rv.rsDrawShower.SetActive(false);
                    rv.rsWinShower.SetActive(false);
                }
                else
                {
                    rv.rsDrawShower.SetActive(true);
                    rv.rsLoseShower.SetActive(false);
                    rv.rsWinShower.SetActive(false);
                }

                StartCoroutine(UpdatePlayerScore());

                rv.resultsPanel.SetActive(true);
                rv.gamePanel.SetActive(false);
            }

            resultsTime -= Time.deltaTime;

            if (resultsTime >= 0)
            {
                rv.rsTimer.value = resultsTime;
                rv.rsTime.text = resultsTime.ToString("N1");
            }
            else
            {
                plScore = 0;  
                oppScore = 0;
                rv.gaPlScore.text = "0";
                rv.gaOppScore.text = "0";
                resultsTime = 5;
                round = 0;
                oppUsername = "";
                canShowResults = false;
                PlayerCont.busy = false;
                rv.resultsPanel.SetActive(false);
            }
        }
    }

    private void DisconnectSocket()
    {
        if (socket != null)
        {
            Challenge challenge = new Challenge();
            challenge.player = PlayerCont.username;
            challenge.opponent = oppUsername;
            challenge.cancelled = true;

            socket.Emit("CHALLENGE_ACCREJ", JsonUtility.ToJson(challenge));

            socket.Disconnect();
            socket = null;
        }
    }
}

public class Challenge
{
    public string player;
    public string opponent;
    public bool accepted;
    public int[] questions;
    public int points;
    public bool male;
    public bool cancelled;
}
