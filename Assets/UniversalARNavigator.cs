using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;        

public class UniversalARNavigator : MonoBehaviour
{
    [Header("Навигация")]
    public NavMeshAgent agent;
    public LineRenderer pathLine;
    public List<Transform> roomPoints;
    public Transform mapHolder;

    [Header("UI Панели")]
    public GameObject calibrationPanel;
    public GameObject selectionPanel;
    public GameObject arrivalPanel;

    [Header("Автоматизация UI")] 
    public Button buttonTemplate;    
    public Transform buttonsContainer;  

    private bool isNavigating = false;

    void Start()
    {
        calibrationPanel.SetActive(true);
        selectionPanel.SetActive(false);
        arrivalPanel.SetActive(false);
        pathLine.enabled = false;
    }


    public void GenerateRoomButtons()
    {

        foreach (Transform child in buttonsContainer)
        {

            if (child.gameObject != buttonTemplate.gameObject)
            {
                Destroy(child.gameObject);
            }
        }

        for (int i = 0; i < roomPoints.Count; i++)
        {
            Button newButton = Instantiate(buttonTemplate, buttonsContainer);
            newButton.gameObject.SetActive(true);


            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = roomPoints[i].name;
            }

            int index = i;
            newButton.onClick.AddListener(() => SelectRoom(index));
        }


        selectionPanel.SetActive(true);
    }



    public void CalibrateAndStart()
    {
        mapHolder.position = transform.position;
        Vector3 forward = transform.forward;
        forward.y = 0;
        mapHolder.rotation = Quaternion.LookRotation(forward);

        calibrationPanel.SetActive(false);
        GenerateRoomButtons(); 
    }


    public void SelectRoom(int index)
    {
        if (index < 0 || index >= roomPoints.Count) return;

        selectionPanel.SetActive(false);
        agent.SetDestination(roomPoints[index].position);
        pathLine.enabled = true;
        isNavigating = true;

        StopAllCoroutines();
        StartCoroutine(ArrivalRoutine());
    }

    void Update()
    {
        if (isNavigating && agent.hasPath)
        {
            pathLine.positionCount = agent.path.corners.Length;
            pathLine.SetPositions(agent.path.corners);
        }
    }

    IEnumerator ArrivalRoutine()
    {
        while (agent.pathPending || agent.remainingDistance > 0.5f)
            yield return null;

        isNavigating = false;
        pathLine.enabled = false;
        arrivalPanel.SetActive(true);
    }


    public void ResetToSelection()
    {
        arrivalPanel.SetActive(false);
        GenerateRoomButtons(); 
        agent.ResetPath();
    }
}