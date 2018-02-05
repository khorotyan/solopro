using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerCont : MonoBehaviour
{
    private ReferenceVariables rv;

    [System.NonSerialized]
    public string token = "";
    [System.NonSerialized]
    public int noWins = 0;

    public static string email = "";
    public static string username = "";
    public static bool online = false;
    public static bool busy = false;

    public static bool inNotPanel = false;
    public static bool inChallPanel = false;

    private void Awake()
    {
        rv = transform.GetChild(0).GetComponent<ReferenceVariables>();

        rv.reqURL = "http://localhost:3000";
        //rv.reqURL = "2acef084.ngrok.io";
        //rv.reqURL = "https://astroffense-resistance.herokuapp.com";

        email = transform.GetComponent<LoadManager>().LoadEmail();
        
        rv.maLogoutButton.onClick.AddListener(delegate { StartCoroutine(logout(bttnClick: true)); });
    } 

    private IEnumerator logout(bool bttnClick = false)
    {
        WWWForm form = new WWWForm();

        form.AddField("email", email);

        Dictionary<string, string> headers = form.headers;
        headers.Add("Authorization", "Bearer " + token);

        WWW www = new WWW(rv.reqURL + "/users/logout", form.data, headers);
        yield return www; 

        if (www.error == null)
        {
            string jsonData = www.text;

            if (bttnClick == true)
            {
                transform.GetComponent<SaveManager>().SaveAutoLogin(false);
            }

            online = false;
            rv.siPasswordInput.text = "";
            rv.signinPanel.SetActive(true);
            rv.mainPage.SetActive(false);
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    private void OnApplicationQuit()
    {
        StartCoroutine(logout());
    }
}

public class UsersList
{
    public List<User> users { get; set; }
}

public class User
{
    public string username;
    public int wins;
    public string email;
    public string password;
    public bool male;
    public bool online;
    public string token;
}
