using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadManager : MonoBehaviour
{
    // Load the player email
    public string LoadEmail()
    {
        string email = "";

        if (ES2.Exists("smfi?tag=email"))
        {
            email = ES2.Load<string>("smfi?tag=email");
        }

        return email;
    }
    
    // Load the autologin indicator
    public bool LoadAutologin()
    {
        bool autologin = false;

        if (ES2.Exists("smfi?tag=autologin"))
        {
            autologin = ES2.Load<bool>("smfi?tag=autologin");
        }

        return autologin;
    }
}
