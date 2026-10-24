using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>Opens the crypt after all relics are collected, then activates its guardian.</summary>
public sealed class CryptGuardianSequence : MonoBehaviour
{
    private const string CounterName = "RelicsCollected";

    [SerializeField] private GameObject cryptDoor;
    [SerializeField] private Collider doorCollider;
    [SerializeField] private LizardmanGuardian guardian;
    [SerializeField] private RelicGameManager gameManager;
    [SerializeField, Min(1)] private int relicGoal = 5;
    [SerializeField] private float openingAngle = 90f;
    [SerializeField, Min(0.1f)] private float openingDuration = 1.2f;

    private bool openingStarted;

    private void Awake()
    {
        if (guardian != null)
        {
            guardian.BindGameManager(gameManager);
            guardian.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (openingStarted)
        {
            return;
        }

        VariableDeclarations sceneVariables = Variables.Scene(gameObject);
        if (sceneVariables == null || !sceneVariables.IsDefined(CounterName))
        {
            return;
        }

        object value = sceneVariables.Get(CounterName);
        if (value != null && Convert.ToInt32(value) >= relicGoal)
        {
            openingStarted = true;
            if (gameManager != null)
            {
                gameManager.ShowStoryMessage("THE SEAL IS RESTORED");
            }

            StartCoroutine(OpenCrypt());
        }
    }

    private IEnumerator OpenCrypt()
    {
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }

        if (cryptDoor != null)
        {
            Transform doorTransform = cryptDoor.transform;
            Quaternion closedRotation = doorTransform.localRotation;
            Quaternion openRotation = closedRotation * Quaternion.Euler(0f, openingAngle, 0f);
            float elapsed = 0f;

            while (elapsed < openingDuration)
            {
                elapsed += Time.deltaTime;
                float blend = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / openingDuration));
                doorTransform.localRotation = Quaternion.Slerp(closedRotation, openRotation, blend);
                yield return null;
            }

            doorTransform.localRotation = openRotation;
        }

        if (gameManager != null)
        {
            gameManager.ShowStoryMessage("THE CRYPT HAS OPENED");
        }

        if (guardian != null)
        {
            guardian.gameObject.SetActive(true);
        }
    }
}
