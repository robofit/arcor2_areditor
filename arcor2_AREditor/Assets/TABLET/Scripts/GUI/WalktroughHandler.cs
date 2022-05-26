using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;

public class WalktroughHandler : MonoBehaviour {

    public List<GameObject> Panels = new List<GameObject> ();

    public WalktroughStep WalktroughStep;

    public WalktroughManager Manager;

    public GameObject WalktroughOverlay;

    public GameObject PreviousButton;

    public GameObject NextButton;

    public GameObject SkipButton;

    public GameObject HighlightingFrame;

    public ProgressBar ProgressBar;

    public GameObject AddNewScene;

    public GameObject TipPanel;

    public bool Initialized;

    public int SkipToStep;

    public GameObject GoOnlineButton;

    public void Awake() {
        if (Initialized == false)
            Initialized = true;
        foreach (Transform child in WalktroughOverlay.transform) {
            Panels.Add(child.gameObject);
        }
    }

    public bool StepInBounds() {
        if (Manager.WalktroughSteps.Count > Manager.Order)
            return true;
        else return false;
    }

    public void DisableOverlay() {
        WalktroughOverlay.SetActive(false);
        var canvasGroup = WalktroughOverlay.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        Manager.Order = 0;
    }

    public void EnableOverlay() {
        WalktroughOverlay.SetActive(true);
        var canvasGroup = WalktroughOverlay.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1;
        Manager.Order = 1;
    }

    public void HandlePreviousButton() {
        if (WalktroughStep.Progress is -1) {
            PreviousButton.GetComponent<CanvasGroup>().interactable = false;
        } else {
            PreviousButton.GetComponent<CanvasGroup>().interactable = true;
        }
    }

    public void HideTipPanel() {
        TipPanel.GetComponent<CanvasGroup>().alpha = 0;
    }

    public void StartWalktrough() {
        CancelHighlighting();
        EnableOverlay();

        WalktroughStep = Manager.WalktroughSteps[Manager.Order];

        if (Manager.Buttons[Manager.Order] != null) {
            WalktroughStep.HighlitedButton = Manager.Buttons[Manager.Order];
        }

        UpdateProgress();
        AddNewListener();
        HandleVisibility();
    }

    public void AddNewListener() {
        if (WalktroughStep.HighlitedButton != null) {
            try {
                NextButton.GetComponent<CanvasGroup>().interactable = false;
                WalktroughStep.HighlitedButton.GetComponent<Button>().onClick.AddListener(StepOver);
            } catch (System.Exception) {
                NextButton.GetComponent<CanvasGroup>().interactable = true;
            }
        } else {
            NextButton.GetComponent<CanvasGroup>().interactable = true;
        }
    }

    public void RemoveLastListener() {
        if (WalktroughStep.HighlitedButton != null) {
            try {
                WalktroughStep.HighlitedButton.GetComponent<Button>().onClick.RemoveListener(StepOver);
            } catch (System.Exception) {
            }
        }
    }

    public void HandleSkip() {
        Manager.Order = SkipToStep;
        StepOver();
    }


    public void StepOver (){
        CancelHighlighting();
        RemoveLastListener();

        if (StepInBounds()) {
            Manager.Order++;
        } else {
            DisableOverlay();
        }

        WalktroughStep = Manager.WalktroughSteps[Manager.Order];

        HandlePreviousButton();

        if (Manager.Order <= Manager.Buttons.Count) {
            if (Manager.Buttons[Manager.Order] != null) {
                WalktroughStep.HighlitedButton = Manager.Buttons[Manager.Order];
            }
        }

        UpdateProgress();
        AddNewListener();
        HandleVisibility();
    }

    public void StepBack() {
        CancelHighlighting();
        RemoveLastListener();

        if (Manager.Order != 0) {
            Manager.Order--;
        }
        WalktroughStep = Manager.WalktroughSteps[Manager.Order];

        HandlePreviousButton();

        UpdateProgress();
        AddNewListener();
        HandleVisibility();
    }

    public async void HandleVisibility() {

        HandlePanel (WalktroughStep.PrimaryText, (int) PanelTypes.PrimaryText);

        HandlePanel(WalktroughStep.SecondaryText, (int) PanelTypes.SecondaryText);

        HandlePanel (WalktroughStep.Tip, (int) PanelTypes.Tip);

        HandleSkipButton();

        StartCoroutine(EnsureButtonHiglighted());
    }

    public void HandleSkipButton() {

        if (WalktroughStep.Skippable) {
            SkipButton.SetActive(true);
            SkipToStep = WalktroughStep.Progress;
        } else {
            SkipButton.SetActive(false);
        }
    }

    public async void HandlePanel(string panelText, int panel) {

        if (string.IsNullOrEmpty(panelText))
        {
            Panels[panel].GetComponent<CanvasGroup>().alpha = 0;
        }
        else
        {
            Panels[panel].GetComponent<CanvasGroup>().alpha = 1;
            TextMeshProUGUI textComponent = Panels[panel].transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            textComponent.text = panelText;

        }
    }
    public IEnumerator EnsureButtonHiglighted() {
        yield return new WaitForSeconds(0.45f);
        HighligthButton();
    }
    public void HighligthButton() {
        if (WalktroughStep.HighlitedButton != null) {
            
            var buttonPosition = (RectTransform) WalktroughStep.HighlitedButton.gameObject.transform;


            ((RectTransform) HighlightingFrame.gameObject.transform).sizeDelta = buttonPosition.rect.size + new Vector2(20,20);
            ((RectTransform) HighlightingFrame.gameObject.transform).pivot = buttonPosition.pivot;

            HighlightingFrame.gameObject.transform.position = buttonPosition.position ;

            if (WalktroughStep.HighlitedButton == AddNewScene) {
                HandleProblematicButton(buttonPosition);
            }
            if (WalktroughStep.HighlitedButton == GoOnlineButton) {
                ((RectTransform) HighlightingFrame.gameObject.transform).sizeDelta = ((RectTransform) buttonPosition.gameObject.transform).rect.size + new Vector2(100, 100);
                ((RectTransform) HighlightingFrame.gameObject.transform).pivot = ((RectTransform) buttonPosition.gameObject.transform).pivot;
                HighlightingFrame.gameObject.transform.position = buttonPosition.gameObject.transform.position;
            }

            HighlightingFrame.SetActive(true);
        }
    }

    private void HandleProblematicButton(RectTransform buttonPosition) {
        ((RectTransform) HighlightingFrame.gameObject.transform).sizeDelta = ((RectTransform) buttonPosition.GetChild(0).gameObject.transform).rect.size + new Vector2(100, 80);
        ((RectTransform) HighlightingFrame.gameObject.transform).pivot = ((RectTransform) buttonPosition.GetChild(0).gameObject.transform).pivot;
        HighlightingFrame.gameObject.transform.position = buttonPosition.GetChild(0).gameObject.transform.position;
    }

    public void CancelHighlighting() {
        HighlightingFrame.SetActive(false);
    }

    public void UpdateProgress() {
        Manager.Progress = (float) Manager.Order / Manager.WalktroughSteps.Count * 100;

        ProgressBar.currentPercent = Manager.Progress;
    }



    public enum PanelTypes {
        PrimaryText,
        SecondaryText,
        Tip
    }
}
