using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class UniversalARNavigator : MonoBehaviour
{
    [Header("AR Components")]
    public ARCloudAnchorManager cloudManager;
    public ARRaycastManager raycastManager;
    public ARAnchorManager anchorManager;

    [Header("Navigation & Offset")]
    public Transform anchorOffset;
    public NavMeshAgent agent;
    public LineRenderer pathLine;
    public List<Transform> roomPoints; // Точки из Blender сюда!
    public TMP_Dropdown roomDropdown;

    [Header("UI & Status")]
    public GameObject adminPanel;
    public GameObject selectionPanel;
    public GameObject arrivalPanel;
    public TextMeshProUGUI statusText;

    private string savedAnchorId;
    private bool isNavigating = false;

    void Awake() => savedAnchorId = PlayerPrefs.GetString("GlobalAnchorID", "");

    void Start()
    {
        pathLine.enabled = false;
        if (string.IsNullOrEmpty(savedAnchorId))
        {
            statusText.text = "Режим: Админ. Создайте якорь у входа.";
            adminPanel.SetActive(true);
        }
        else
        {
            statusText.text = "Статус: Поиск якоря...";
            StartCoroutine(ResolveAnchorRoutine());
        }
    }

    public void CreateAndHostAnchor()
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits))
        {
            statusText.text = "Статус: Хостинг в облако...";
            var anchor = anchorManager.AddAnchor(hits.pose);
            var promise = cloudManager.HostCloudAnchorAsync(anchor, 365);
            StartCoroutine(WaitHosting(promise));
        }
    }

    IEnumerator WaitHosting(HostCloudAnchorPromise promise)
    {
        yield return promise;
        if (promise.Result.CloudAnchorState == CloudAnchorState.Success)
        {
            savedAnchorId = promise.Result.CloudAnchorId;
            PlayerPrefs.SetString("GlobalAnchorID", savedAnchorId);
            adminPanel.SetActive(false);
            StartCoroutine(ResolveAnchorRoutine());
        }
        else statusText.text = "Ошибка: " + promise.Result.CloudAnchorState;
    }

    IEnumerator ResolveAnchorRoutine()
    {
        var promise = cloudManager.ResolveCloudAnchorAsync(savedAnchorId);
        yield return promise;

        if (promise.Result.CloudAnchorState == CloudAnchorState.Success)
        {
            statusText.text = "Статус: Успех! Выберите кабинет.";
            anchorOffset.position = promise.Result.Anchor.transform.position;
            anchorOffset.rotation = promise.Result.Anchor.transform.rotation;
            selectionPanel.SetActive(true);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
            StartCoroutine(ResolveAnchorRoutine());
        }
    }

    public void StartNavigation() // Привязать к Dropdown (On Value Changed)
    {
        int index = roomDropdown.value;
        selectionPanel.SetActive(false);
        arrivalPanel.SetActive(false);
        agent.SetDestination(roomPoints[index].position);
        pathLine.enabled = true;
        isNavigating = true;
        StartCoroutine(ArrivalCheck());
    }

    void Update()
    {
        if (isNavigating && agent.hasPath)
        {
            pathLine.positionCount = agent.path.corners.Length;
            pathLine.SetPositions(agent.path.corners);
        }
    }

    IEnumerator ArrivalCheck()
    {
        while (agent.pathPending || agent.remainingDistance > 0.6f) yield return null;
        isNavigating = false;
        pathLine.enabled = false;
        arrivalPanel.SetActive(true);
    }

    public void OpenSelection() { arrivalPanel.SetActive(false); selectionPanel.SetActive(true); agent.ResetPath(); }
}
