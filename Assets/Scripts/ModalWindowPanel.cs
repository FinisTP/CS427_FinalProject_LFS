using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ModalWindowPanel : MonoBehaviour
{
    public GameObject ModalWindowBox;
    public static ModalWindowPanel Instance;

    [Header("Header")]
    public Transform HeaderArea;
    public TMP_Text TitleField;

    [Header("Content")]
    public Transform ContentArea;
    public Transform VerticalLayoutArea;
    public Image ContentImage;
    public TMP_Text ContentText;

    [Header("Footer")]
    public Transform FooterArea;
    public Button ConfirmButton;
    public TMP_Text ConfirmMessage;
    public Button CancelButton;
    public TMP_Text CancelMessage;

    [Header("Button Actions")]
    private Action onConfirmAction;
    private Action onDeclineAction;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ModalWindowBox.SetActive(false);
    }

    public void Confirm()
    {
        onConfirmAction?.Invoke();
        if (onConfirmAction == null) ModalWindowBox.SetActive(false);
    }

    public void Decline()
    {
        onDeclineAction?.Invoke();
        if (onConfirmAction == null) ModalWindowBox.SetActive(false);
    }


    public void ShowModal(string title = "", Sprite imageToShow = null, string message = "", string confirmMessage = "",
        string declineMessage = "", Action confirmAction = null, Action declineAction = null)
    {
        ModalWindowBox.SetActive(true);

        bool hasTitle = String.IsNullOrEmpty(title);
        HeaderArea.gameObject.SetActive(hasTitle);
        TitleField.text = title;

        bool hasImage = imageToShow != null;
        ContentImage.gameObject.SetActive(hasImage);
        if (hasImage) ContentImage.sprite = imageToShow;
        ContentText.text = message;

        onConfirmAction = confirmAction;
        ConfirmMessage.text = confirmMessage;

        bool hasDecline = declineAction != null;
        CancelButton.gameObject.SetActive(hasDecline);
        onDeclineAction = declineAction;
        CancelMessage.text = declineMessage;
    }

}
