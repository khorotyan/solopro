using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChallengeCont : MonoBehaviour
{
    private ReferenceVariables rv;

    private float challRefTime = 0;

    private void Awake()
    {
        rv = transform.GetChild(0).GetComponent<ReferenceVariables>();

        rv.maPlayButton.onClick.AddListener(delegate { OnMainPlayClick(); });

        rv.ntCloseButton.onClick.AddListener(delegate { OnNotifPanelClose(); });

        rv.ntChallengeButton.onClick.AddListener(delegate { OnChallengeClick(); });

        rv.chCloseButton.onClick.AddListener(delegate { OnChallPageClose(); });
    }

    private void Update()
    {
        RefreshChallPlayers();
    }

    private void OnMainPlayClick()
    {
        PlayerCont.inNotPanel = true;

        rv.playPage.SetActive(true);
        rv.notificationsPanel.SetActive(true);
    }

    private void OnNotifPanelClose()
    {
        PlayerCont.inNotPanel = false;

        rv.playPage.SetActive(false);
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

        rv.playPage.SetActive(false);
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

            for (int i = 0; i < sc.users.Count; i++)
            {
                if (sc.users[i].username != transform.GetComponent<PlayerCont>().username)
                {
                    Transform challUser = Instantiate(rv.challengesObj, rv.chChallsContainer).transform;

                    challUser.GetChild(0).GetComponent<Image>().sprite = sc.users[i].male == false ? rv.femaleSprite : rv.maleSprite;
                    challUser.GetChild(1).GetComponent<Text>().text = sc.users[i].username;
                    challUser.GetChild(2).GetComponent<Text>().text = "Wins: " + sc.users[i].wins;
                    challUser.GetChild(3).GetComponent<Button>().onClick.AddListener(delegate { InitiateChallenge(); });
                    
                    if (sc.users[i].online == true)
                    {
                        challUser.GetChild(4).GetChild(2).gameObject.SetActive(true);
                        challUser.GetChild(4).GetChild(1).gameObject.SetActive(false);
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

    private void InitiateChallenge()
    {

    }
}
