using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
public class UIGameManager : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button submitButton;

    public GameObject LoginPanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        submitButton.onClick.AddListener(OnSubmitName);
        LoginPanel.SetActive(false);

        GameManager2.Instance.OnConnection += () =>
        {
            LoginPanel.SetActive(true);
            inputField.text = "";
            submitButton.interactable = true;
            inputField.interactable = true;
        };
    }
    public void OnSubmitName()
    {
        string accountID = inputField.text;
        if (!string.IsNullOrEmpty(accountID))
        {
            GameManager2.Instance.RegisterPlayerServerRpc(accountID, NetworkManager.Singleton.LocalClientId);
            submitButton.interactable = false;
            inputField.interactable = false;

            LoginPanel.SetActive(false);
        }
    }
}
