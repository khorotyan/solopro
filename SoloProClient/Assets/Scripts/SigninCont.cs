using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SigninCont : MonoBehaviour
{
    private ReferenceVariables rv;
    private SaveManager saveManager;
    private LoadManager loadManager;

    private void Awake()
    {
        rv = transform.GetChild(0).GetComponent<ReferenceVariables>();
        saveManager = transform.GetComponent<SaveManager>();
        loadManager = transform.GetComponent<LoadManager>();

        rv.siPasswordInput.contentType = InputField.ContentType.Password;

        rv.siLoginButton.onClick.AddListener(delegate { OnLoginClick(); });

        rv.siSignupPrButton.onClick.AddListener(delegate { OnSignupClick(); });

        AutoLoginPlayer();
    }

    private void AutoLoginPlayer()
    {
        if (transform.GetComponent<LoadManager>().LoadAutologin() == true)
        {
            StartCoroutine(Login(autoLogin: true));
        }
        else
        {
            rv.siEmailInput.text = transform.GetComponent<LoadManager>().LoadEmail();
        }
    }

    // Login or check for errors whenever the login button is clicked
    private void OnLoginClick()
    {
        if (rv.siEmailInput.text.Length == 0)
        {
            rv.ShowInfo("Please fill the email field");
        }
        else if (rv.siPasswordInput.text.Length == 0)
        {
            rv.ShowInfo("Please fill the password field");
        }
        else
        {
            StartCoroutine(Login());
        }
    }

    // Login to the player account either automatically or manually
    public IEnumerator Login(bool autoLogin = false)
    {
        WWWForm form = new WWWForm();

        if (autoLogin == true)
        {
            form.AddField("email", transform.GetComponent<PlayerCont>().email);
            form.AddField("autologin", 1);
        }
        else
        {
            form.AddField("email", rv.siEmailInput.text);
            form.AddField("autologin", 0);
            form.AddField("password", rv.siPasswordInput.text);
        }
        
        UnityWebRequest www = UnityWebRequest.Post(rv.reqURL + "/users/login", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            rv.ShowInfo("Authentication error");
        }
        else
        {
            string jsonData = www.downloadHandler.text;

            User user = JsonUtility.FromJson<User>(jsonData);

            transform.GetComponent<PlayerCont>().token = user.token;
            transform.GetComponent<PlayerCont>().username = user.username;
            transform.GetComponent<PlayerCont>().noWins = user.wins;

            rv.maUsernameText.text = user.username;
            if (user.male == false)
            {
                rv.maUserIcon.sprite = rv.femaleSprite;
                rv.maUserIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-390.5f, 822f);
            }
            else
            {
                rv.maUserIcon.sprite = rv.maleSprite;
                rv.maUserIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-390.5f, 828f);
            }
            rv.maNoWinsText.text = "Wins: " + user.wins;

            if (autoLogin == false)
            {
                saveManager.SaveAutoLogin(rv.siAutoLoginToggle.isOn);
                saveManager.SaveEmail(rv.siEmailInput.text);
            }
            else
            {
                rv.signupPanel.SetActive(false);
            }

            rv.mainPage.SetActive(true);
            rv.signinPanel.SetActive(false);
        }
    }

    // Open signup and close login panel 
    private void OnSignupClick()
    {
        rv.signupPanel.SetActive(true);
        rv.signinPanel.SetActive(false);
    }
}

