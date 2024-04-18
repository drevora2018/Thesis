using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Targets : MonoBehaviour
{
    public PauseMenu pauseMenu;
    public UserTestLog userTestLog;
    public GameObject[] TargetBoxes;
    public GameObject[] TargetBoxesMirror;
    GameObject[] ActiveTargetBoxes;
    public List<float> distances = new ();
    public Image[] MapTargets;
    public Image[] MapTargetsMirror;
    Image[] ActiveMapTargets;
    public GameObject TruckTarget;
    public Image TruckMapTarget;
    public Color baseColor;
    public Color highlightColor = Color.white;

    int targetIndex = 0;
    int targetCount;
    bool firstTarget = true;

    void Start()
    {
        if (PlayerPrefs.GetInt("Scenario") % 2 == 0)
        {
            ActiveTargetBoxes = TargetBoxes;
            ActiveMapTargets = MapTargets;
        }
        else
        {
            ActiveTargetBoxes = TargetBoxesMirror;
            ActiveMapTargets = MapTargetsMirror;
        }

        foreach (var target in TargetBoxes)
        {
            target.SetActive(false);
        }
        foreach (var target in TargetBoxesMirror)
        {
            target.SetActive(false);
        }

        foreach (var mapTarget in MapTargets)
        {
            mapTarget.color = baseColor;
        }
        foreach (var mapTarget in MapTargetsMirror)
        {
            mapTarget.color = baseColor;
        }

        TruckMapTarget.color = highlightColor;
        TruckTarget.GetComponent<MeshRenderer>().enabled = false;
        TruckTarget.SetActive(true);
        targetCount = ActiveTargetBoxes.Length;
    }
    void Update()
    {
        
    }

    public void TargetReached(GameObject container)
    {
        userTestLog.AddTargetDistance(Vector3.Distance(container.transform.position, ActiveTargetBoxes[targetIndex].transform.position));
        ActiveMapTargets[targetIndex].color = baseColor;
        ActiveTargetBoxes[targetIndex].SetActive(false);
        TruckMapTarget.color= highlightColor;
        TruckTarget.SetActive(true);
        if (targetIndex >= targetCount-1)
        {
            //last target reached
            userTestLog.LapTime();
            userTestLog.EndTaskTimer();
            pauseMenu.ManualTogglePauseMenu();
            userTestLog.OutputLog();
            //TruckTarget.GetComponent<MeshRenderer>().enabled = true;
        }

    }

    public void NextTarget()
    {
        targetIndex++;
        if (firstTarget)
        {
            firstTarget = false;
            targetIndex = 0;
            userTestLog.StartTaskTimer();
        }
        else
        {
            userTestLog.LapTime();
        }

        if (targetIndex < targetCount)
        {
            ActiveMapTargets[targetIndex].color = highlightColor;
            ActiveTargetBoxes[targetIndex].SetActive(true);
            TruckMapTarget.color = baseColor;
            TruckTarget.SetActive(false);
        }
        else
        {
            //end task??? all boxes done
            userTestLog.EndTaskTimer();
            pauseMenu.ManualTogglePauseMenu();
            userTestLog.OutputLog();
        }
    }
}
