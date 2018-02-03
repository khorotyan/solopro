using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    // Encrypt and save the player email
    public void SaveEmail(string email)
    {
        ES2.Save(email, "smfi?tag=email");
    }

    // Encrypt and save the toggle indicating whether the player will  
    //  automatically or manually login to his/her account
    public void SaveAutoLogin(bool autologin)
    {
        ES2.Save(autologin, "smfi?tag=autologin");
    }
}
