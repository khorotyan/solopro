using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SignupCont : MonoBehaviour
{
    private ReferenceVariables rv;

    // Set button, input and other behaviors at the beginning of the application
    private void Awake()
    {
        // Get a reference to the class containing the references to most of the variables
        rv = transform.GetChild(0).GetComponent<ReferenceVariables>();

        rv.suUsernameInput.onEndEdit.AddListener(delegate { ValidateUsername(); });
        rv.suUsernameInput.characterLimit = 15;

        rv.suEmailInput.onEndEdit.AddListener(delegate { ValidateEmail(); });
        rv.suEmailInput.characterLimit = 30;

        rv.suPasswordInput.onEndEdit.AddListener(delegate { ValidatePassword(); CheckPassMatch(); });
        rv.suPasswordInput.characterLimit = 30;
        rv.suPasswordInput.contentType = InputField.ContentType.Password;

        rv.suPassConfInput.onEndEdit.AddListener(delegate { CheckPassMatch(); });
        rv.suPassConfInput.characterLimit = 30;
        rv.suPassConfInput.contentType = InputField.ContentType.Password;

        rv.suCrAccountButton.onClick.AddListener(delegate { OnCreateAccountClick(); });

        rv.suSigninPrButton.onClick.AddListener(delegate { OnSigninClick(); });
    }

    private void ValidateUsername()
    {
        string pattern = @"^(?!.*[_\s-]{2,})[a-zA-Z][a-zA-Z0-9_\s\-]*[a-zA-Z0-9]$";

        if (rv.suUsernameInput.text.Length < 3)
        {
            rv.suUsernameInput.text = "";
            rv.ShowInfo("Username must be at least 3 letters long");     
            rv.suUsernameInput.GetComponent<Image>().sprite = rv.inpWrongSp;
        }

        if (Regex.IsMatch(rv.suUsernameInput.text, pattern))
        {
            // Everything is okay
            rv.suUsernameInput.GetComponent<Image>().sprite = rv.inpDefaultSp;
        }
        else
        {
            rv.suUsernameInput.text = "";
            rv.ShowInfo("Please write a valid username");
            rv.suUsernameInput.GetComponent<Image>().sprite = rv.inpWrongSp;
        }
    }

    private void ValidateEmail()
    {
        string pattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

        if (Regex.IsMatch(rv.suEmailInput.text, pattern))
        {
            // Everything is okay
            rv.suEmailInput.GetComponent<Image>().sprite = rv.inpDefaultSp;
        }
        else
        {
            rv.suEmailInput.text = "";
            rv.ShowInfo("Please write a valid email");
            rv.suEmailInput.GetComponent<Image>().sprite = rv.inpWrongSp;
        }
    }

    private void ValidatePassword()
    {
        if (rv.suPasswordInput.text.Length < 4)
        {
            rv.suPasswordInput.text = "";
            rv.ShowInfo("Password must be at least 4 letters long");
            rv.suPasswordInput.GetComponent<Image>().sprite = rv.inpWrongSp;
        }
        else
        {
            // Everything is okay
            rv.suPasswordInput.GetComponent<Image>().sprite = rv.inpDefaultSp;
        }
    }

    private void CheckPassMatch()
    {
        if (rv.suPasswordInput.text.Length > 0 && rv.suPassConfInput.text.Length > 0)
        {
            if (rv.suPasswordInput.text != rv.suPassConfInput.text)
            {
                rv.suPasswordInput.text = "";
                rv.suPassConfInput.text = "";
                rv.ShowInfo("Passwords do not match");
                rv.suPasswordInput.GetComponent<Image>().sprite = rv.inpWrongSp;
                rv.suPassConfInput.GetComponent<Image>().sprite = rv.inpWrongSp;
            }
            else
            {
                // Everything is okay
                rv.suPasswordInput.GetComponent<Image>().sprite = rv.inpDefaultSp;
                rv.suPassConfInput.GetComponent<Image>().sprite = rv.inpDefaultSp;
            }
        }      
    }

    private void OnCreateAccountClick()
    {
        if (rv.suUsernameInput.text.Length == 0)
        {
            rv.ShowInfo("Please fill the username field");
        }
        else if (rv.suEmailInput.text.Length == 0)
        {
            rv.ShowInfo("Please fill the email field");
        }
        else if (rv.suPasswordInput.text.Length == 0 || rv.suPassConfInput.text.Length == 0)
        {
            rv.ShowInfo("Please fill the password fields");
        }
        else if (rv.suMaleToggle.isOn == false && rv.suFemaleToggle.isOn == false)
        {
            rv.ShowInfo("Please select your gender");
        }
        else
        {
            StartCoroutine(Signup());
        }
    }

    // Login to the player account either automatically or manually
    public IEnumerator Signup()
    {
        WWWForm form = new WWWForm();

        form.AddField("username", rv.suUsernameInput.text);
        form.AddField("email", rv.suEmailInput.text);
        form.AddField("password", rv.suPasswordInput.text);
        form.AddField("male", rv.suMaleToggle.isOn == true ? 1 : 0);

        UnityWebRequest www = UnityWebRequest.Post(rv.reqURL + "/users/signup", form);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);

            if (www.downloadHandler.text.Contains("Username already exists"))
            {
                rv.ShowInfo("Username already exists");
            }
            else
            {
                rv.ShowInfo("Network error");
            }
        }
        else
        {
            string jsonData = www.downloadHandler.text;

            User user = JsonUtility.FromJson<User>(jsonData);
            transform.GetComponent<PlayerCont>().token = user.token;
            transform.GetComponent<PlayerCont>().noWins = 0;
            PlayerCont.email = rv.suEmailInput.text;
            PlayerCont.username = rv.suUsernameInput.text;

            transform.GetComponent<SaveManager>().SaveAutoLogin(rv.suAutoLoginToggle.isOn);
            transform.GetComponent<SaveManager>().SaveEmail(rv.suEmailInput.text);

            rv.suPasswordInput.text = "";
            rv.suPassConfInput.text = "";
            rv.siEmailInput.text = rv.suEmailInput.text;

            // Set main page info
            rv.maUsernameText.text = rv.suUsernameInput.text;
            if (rv.suMaleToggle.isOn == false)
            {
                rv.maUserIcon.sprite = rv.femaleSprite;
                rv.maUserIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-390.5f, 822f);
            }
            else
            {
                rv.maUserIcon.sprite = rv.maleSprite;
                rv.maUserIcon.GetComponent<RectTransform>().anchoredPosition = new Vector2(-390.5f, 828f);
            }
            rv.maNoWinsText.text = "Wins: 0";

            PlayerCont.online = true;
            rv.mainPage.SetActive(true);
            rv.signupPanel.SetActive(false);
        }
    }

    private void OnSigninClick()
    {
        rv.signinPanel.SetActive(true);
        rv.signupPanel.SetActive(false);
    }
}
