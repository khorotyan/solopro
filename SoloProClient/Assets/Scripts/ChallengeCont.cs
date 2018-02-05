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

    private Socket socket;

    private string oppUsername = "";

    private bool waitingForOpponent = false;
    private bool waitingForChallAcc = false;
    private bool waitTillChallStart = false;

    private float challRefTime = 0;
    private float challWaitTime = 5;
    private float challAccTime = 5;
    private float challStartTime = 5;

    private void Awake()
    {
        rv = transform.GetChild(0).GetComponent<ReferenceVariables>();

        rv.maPlayButton.onClick.AddListener(delegate { OnMainPlayClick(); });

        rv.ntCloseButton.onClick.AddListener(delegate { OnNotifPanelClose(); });

        rv.ntChallengeButton.onClick.AddListener(delegate { OnChallengeClick(); });

        rv.chCloseButton.onClick.AddListener(delegate { OnChallPageClose(); });
    }
    
    private void Start()
    {
        socket = IO.Socket(rv.reqURL + "/challenges");

        CheckForChallenge();
        CheckChallAccept();
    }
    
    private void Update()
    {
        RefreshChallPlayers();

        WaitForOpponent();
        WaitForChallenge();

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

                    challUser.GetChild(0).GetComponent<Image>().sprite = sc.users[i].male == false ? rv.femaleSprite : rv.maleSprite;
                    challUser.GetChild(1).GetComponent<Text>().text = sc.users[i].username;
                    challUser.GetChild(2).GetComponent<Text>().text = "Wins: " + sc.users[i].wins;
        
                    if (sc.users[i].online == true)
                    {
                        challUser.GetChild(4).GetChild(2).gameObject.SetActive(true);
                        challUser.GetChild(4).GetChild(1).gameObject.SetActive(false);

                        challUser.GetChild(3).gameObject.SetActive(true);
                        challUser.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { OnChallengeSend(plUsername, oppUsername); });
                    }
                }   
            }
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
    private void OnChallengeSend(string player, string opponent)
    {
        Challenge challenge = new Challenge();
        challenge.player = player;
        challenge.opponent = opponent;

        socket.Emit("CHALLENGE_SENT", JsonUtility.ToJson(challenge));

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
                    // Open the challenge accept panel   
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
                rv.challNotPanel.SetActive(true);

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
            Challenge challenge = new Challenge();
            challenge.player = PlayerCont.username;
            challenge.opponent = oppUsername;
            challenge.accepted = accept;

            socket.Emit("CHALLENGE_ACCREJ", JsonUtility.ToJson(challenge));

            rv.csPlUsername.text = PlayerCont.username.ToString();
            rv.csOppUsername.text = oppUsername;

            waitTillChallStart = true;
        }
        else
        {
            // Cancel the notification of the other player (Waiting ...)

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
                    waitTillChallStart = true;

                    challAccTime = 5;
                    challWaitTime = 5;
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
    }

    private IEnumerator TurnOffLobby()
    {
        yield return new WaitForSeconds(0.5f);

        rv.challStartPanel.SetActive(false);
        rv.gamePanel.SetActive(true);
        waitTillChallStart = false;
        challStartTime = 5;
    }

    private IEnumerator AnimateChallQuestion()
    {
        Color32 imageCol = rv.gaQNumShower.GetComponent<Image>().color;
        imageCol.a = 255;
        rv.gaQNumShower.GetComponent<Image>().DOColor(imageCol, 0.5f);

        Color32 textCol = rv.gaQuestionNum.GetComponent<Text>().color;
        textCol.a = 255;
        rv.gaQuestionNum.GetComponent<Text>().DOColor(textCol, 0.5f);

        yield return new WaitForSeconds(1.5f);

        imageCol.a = 0;
        rv.gaQNumShower.GetComponent<Image>().DOColor(imageCol, 0.5f);

        textCol.a = 0;
        rv.gaQuestionNum.GetComponent<Text>().DOColor(textCol, 0.5f);

        yield return new WaitForSeconds(0.5f);
    }


}

public class Challenge
{
    public string player;
    public string opponent;
    public bool accepted;
}
