using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

[System.Serializable]
public class Message
{
    public string text;
    public string author;
    public TMP_Text chatObject;
    public MessageType messageType;

    public enum MessageType
    {
        PLAYER_MESSAGE,
        WARNING,
        IMPORTANT
    }
}

public class ChatManager : MonoBehaviourPunCallbacks
{
    public int maxMessage = 25;
    public GameObject chatPanel;
    public GameObject chatObject;
    public TMP_InputField chatBox;
    public Color playerMessageColor, warningMessageColor, importantMessageColor;

    [SerializeField]
    List<Message> messageList = new List<Message>();


    private void Update()
    {
        // if (!photonView.IsMine) return;
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                photonView.RPC("SendMessageToChat", RpcTarget.All,
                    PhotonNetwork.LocalPlayer.NickName, chatBox.text, (byte)Message.MessageType.PLAYER_MESSAGE);
                // SendMessageToChat(chatBox.text, Message.MessageType.PLAYER_MESSAGE);
                chatBox.text = "";
            }
        }
        else
        {
            if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatBox.Select();
                chatBox.ActivateInputField();
            }
        }
            
    }

    Color MessageTypeColor(Message.MessageType messageType)
    {
        Color color = warningMessageColor;
        switch (messageType)
        {
            case Message.MessageType.PLAYER_MESSAGE:
                color = playerMessageColor;
                break;
            case Message.MessageType.IMPORTANT:
                color = importantMessageColor;
                break;
            default:
                break;
        }
        return color;
    }

    [PunRPC]
    public void SendMessageToChat(string author, string text, byte messageType)
    {
        if (messageList.Count > maxMessage)
        {
            Destroy(messageList[0].chatObject.gameObject);
            messageList.Remove(messageList[0]);
        }

        Message newMessage = new Message();
        newMessage.text = text;
        newMessage.author = author;

        GameObject newText = Instantiate(chatObject, chatPanel.transform);
        newMessage.chatObject = newText.GetComponent<TMP_Text>();
        newMessage.chatObject.text = $"{newMessage.author}: {newMessage.text}";
        newMessage.chatObject.color = MessageTypeColor((Message.MessageType)messageType);

        messageList.Add(newMessage);
    }
}
