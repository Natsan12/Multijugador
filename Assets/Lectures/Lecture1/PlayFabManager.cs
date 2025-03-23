using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabManager : MonoBehaviour
{
    void Start()
    {
        LoginWithDeviceId();
    }

    void LoginWithDeviceId()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("✅ Login exitoso con PlayFab - ID: " + result.PlayFabId);
    }

    void OnLoginError(PlayFabError error)
    {
        Debug.LogError("❌ Error al iniciar sesión con PlayFab:");
        Debug.LogError(error.GenerateErrorReport());
    }
}
