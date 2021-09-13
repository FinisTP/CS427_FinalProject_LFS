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
    public GameObject chatCanvas;
    public GameObject chatPanel;
    public GameObject chatObject;
    public TMP_InputField chatBox;
    public Color playerMessageColor, warningMessageColor, importantMessageColor;

    private bool _chatEnabled = false;

    [SerializeField]
    List<Message> messageList = new List<Message>();

    private void Start()
    {
        _chatEnabled = false;
        chatCanvas.SetActive(false);
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T) && !chatBox.isFocused)
        {
            _chatEnabled = !_chatEnabled;
            if (_chatEnabled)
            {
                EnableChat();
            } else
            {
                DisableChat();
            }
        }

        if (!_chatEnabled) return;

        // if (!photonView.IsMine) return;
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (ExecuteCommand(chatBox.text))
                photonView.RPC("SendMessageToChat", RpcTarget.All,
                    PhotonNetwork.LocalPlayer.NickName, chatBox.text, (byte)Message.MessageType.WARNING);
                else
                {
                    photonView.RPC("SendMessageToChat", RpcTarget.All,
                    PhotonNetwork.LocalPlayer.NickName, chatBox.text, (byte)Message.MessageType.PLAYER_MESSAGE);
                }
                // SendMessageToChat(chatBox.text, Message.MessageType.PLAYER_MESSAGE);
                chatBox.text = "";
            }
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!chatBox.isFocused)
            {
                chatBox.Select();
                chatBox.ActivateInputField();
            }
            else
            {
                DisableChat();
            }
        } 
    }

    public bool ExecuteCommand(string text)
    {
        if (text.StartsWith(".cmd "))
        {
            // is a command
            string cmd = text.Substring(5);
            if (cmd.StartsWith("p ") || cmd.StartsWith("play "))
            {
                // play video
                photonView.RPC("PlayVideo", RpcTarget.All, cmd.Substring(2));
            }
            else if (cmd.StartsWith("erase"))
            {
                // erase board
                photonView.RPC("EraseBoard", RpcTarget.All);
            }
            return true;
        }
        return false;
    }

    [PunRPC]
    public void PlayVideo(string url)
    {
        OnlineVideoLoader ovl = FindObjectOfType<OnlineVideoLoader>();
        ovl.videoUrl = url;
        ovl.PlayVideo();
    }

    [PunRPC]
    public void EraseBoard()
    {
        FindObjectOfType<Paintable>().EraseBoard();
    }

    public void EnableChat()
    {
        chatCanvas.SetActive(true);
        GameplayManager.instance._isChatting = true;
    }

    public void DisableChat()
    {
        chatCanvas.SetActive(false);
        GameplayManager.instance._isChatting = false;
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
        if (!_chatEnabled) EnableChat();

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
