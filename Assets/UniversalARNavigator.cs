using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class RoomData
{
    public Transform roomPoint; 
    public string russianName; 
}

public class UniversalARNavigator : MonoBehaviour
{
    [Header("Точки комнат и названия")]
    public List<RoomData> roomList;

    [Header("Навигация")]
    public NavMeshAgent agent;
    public LineRenderer pathLine;
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


        for (int i = 0; i < roomList.Count; i++)
        {
            Button newButton = Instantiate(buttonTemplate, buttonsContainer);
            newButton.gameObject.SetActive(true);


            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = roomList[i].russianName; 
            }

  
            int index = i;
            newButton.onClick.AddListener(() => SelectRoom(index));
        }

        selectionPanel.SetActive(true);
    }



    [Header("Привязка")]
    public Transform virtualEntrance; 

    public void CalibrateAndStart()
    {
        Vector3 forward = transform.forward;
        forward.y = 0;
        mapHolder.rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(0, 180, 0);
        Vector3 offset = mapHolder.position - virtualEntrance.position;
        mapHolder.position = transform.position + offset;
        calibrationPanel.SetActive(false);
        GenerateRoomButtons();
    }


    public void SelectRoom(int index)
    {
        if (index < 0 || index >= roomList.Count) return; 

        selectionPanel.SetActive(false);
        agent.SetDestination(roomList[index].roomPoint.position); 
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