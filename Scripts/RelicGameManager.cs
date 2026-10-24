using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Shows the scene counter written by the relic Visual Script graph.</summary>
public sealed class RelicGameManager : MonoBehaviour
{
    private const string CounterName = "RelicsCollected";

    [SerializeField] private Text counterLabel;
    [SerializeField] private GameObject victoryMessage;
    [SerializeField] private GameObject storyPanel;
    [SerializeField, Min(1)] private int relicGoal = 5;
    [SerializeField, Min(0.5f)] private float storyMessageDuration = 3f;

    private VariableDeclarations sceneVariables;
    private Text storyMessageLabel;
    private Coroutine hideMessageRoutine;
    private int displayedCount = -1;
    private bool finalEncounterComplete;

    private void Awake()
    {
        if (counterLabel != null)
        {
            counterLabel.gameObject.SetActive(true);
        }

        if (victoryMessage != null)
        {
            storyMessageLabel = victoryMessage.GetComponent<Text>();
            victoryMessage.SetActive(false);
        }

        if (storyPanel != null)
        {
            storyPanel.SetActive(false);
        }
    }

    private void Start()
    {
        RefreshDisplay();
    }

    private void Update()
    {
        RefreshDisplay();
    }

    public void CompleteFinalEncounter()
    {
        if (finalEncounterComplete)
        {
            return;
        }

        finalEncounterComplete = true;
        StopHideMessageRoutine();
        SetStoryMessage("THE LAST GUARDIAN HAS FALLEN\nVICTORY");
    }

    public void ShowStoryMessage(string message)
    {
        if (finalEncounterComplete || victoryMessage == null || storyMessageLabel == null)
        {
            return;
        }

        StopHideMessageRoutine();
        SetStoryMessage(message);
        hideMessageRoutine = StartCoroutine(HideStoryMessageAfterDelay());
    }

    private void RefreshDisplay()
    {
        int count = GetRelicCount();

        if (count != displayedCount)
        {
            displayedCount = count;
            if (counterLabel != null)
            {
                counterLabel.text = $"SEAL FRAGMENTS\n{count} / {relicGoal}";
            }
        }

        if (counterLabel != null && !counterLabel.gameObject.activeSelf)
        {
            counterLabel.gameObject.SetActive(true);
        }
    }

    private int GetRelicCount()
    {
        // SceneVariables may finish initializing after this component's Awake.
        if (sceneVariables == null)
        {
            sceneVariables = Variables.Scene(gameObject);
        }

        if (sceneVariables == null || !sceneVariables.IsDefined(CounterName))
        {
            return 0;
        }

        object value = sceneVariables.Get(CounterName);
        return value == null ? 0 : Convert.ToInt32(value);
    }

    private void SetStoryMessage(string message)
    {
        if (storyMessageLabel == null || victoryMessage == null)
        {
            return;
        }

        storyMessageLabel.text = message;
        if (storyPanel != null)
        {
            storyPanel.SetActive(true);
        }

        victoryMessage.SetActive(true);
    }

    private IEnumerator HideStoryMessageAfterDelay()
    {
        yield return new WaitForSeconds(storyMessageDuration);

        if (!finalEncounterComplete && victoryMessage != null)
        {
            victoryMessage.SetActive(false);
            if (storyPanel != null)
            {
                storyPanel.SetActive(false);
            }
        }

        hideMessageRoutine = null;
    }

    private void StopHideMessageRoutine()
    {
        if (hideMessageRoutine == null)
        {
            return;
        }

        StopCoroutine(hideMessageRoutine);
        hideMessageRoutine = null;
    }
}
