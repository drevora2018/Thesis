using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class AccuracyFinder : MonoBehaviour
{
    public int TargetContainers;
    public CollisionDetection CollisionDetection;
    ContainerYardScript shipstorage;
    public ManualControlQuay crane;
    public RayCastAroundCrane audio;
    GameObject StatisticsCanvas;
    GameObject ControlUI;

    bool Guidance1, Guidance2;

    public List<Vector3Int> ContainerYardPreInstantiatedList = new List<Vector3Int>();

    // Start is called before the first frame update
    void Start()
    {
        
        shipstorage = GameObject.Find("Ship").GetComponentInChildren<ContainerYardScript>();
        crane = GameObject.FindGameObjectWithTag("PanamaxCrane").GetComponent<ManualControlQuay>();
        audio = GameObject.FindGameObjectWithTag("PanamaxCrane").GetComponent<RayCastAroundCrane>();

        shipstorage.Height = 3;
        shipstorage.Width = 4;
        shipstorage.Length = 4;


        switch (PlayerPrefs.GetInt("Scenario"))
        {
            case 1: // Control Scenario
                crane.ForceFeedback = false;
                audio.EnableCollisionNotification = false;
                audio.EnableGuidanceNotificaton = false;
                break;
            case 2: // Force Feedback Only
                crane.ForceFeedback = true;
                audio.EnableCollisionNotification = false;
                audio.EnableGuidanceNotificaton = false;
                break;
            case 3: // Audio Guidance1
                crane.ForceFeedback = false;
                audio.EnableCollisionNotification = false;
                audio.EnableGuidanceNotificaton = true;
                break;
            case 4: // Audio Guidance2
                crane.ForceFeedback = false;
                audio.EnableCollisionNotification = false;
                audio.EnableGuidanceNotificaton = true;
                break;
            case 5: // Force Feedback and Guidance1
                crane.ForceFeedback = true;
                audio.EnableCollisionNotification = false;
                audio.EnableGuidanceNotificaton = true;
                break;
            case 6: // Force Feedback and Collision
                crane.ForceFeedback = true;
                audio.EnableCollisionNotification = true;
                audio.EnableGuidanceNotificaton = false;
                break;
            case 7: // All Audio on
                crane.ForceFeedback = false;
                audio.EnableCollisionNotification = true;
                audio.EnableGuidanceNotificaton = true;
                break;
            case 8: // Yes
                crane.ForceFeedback = true;
                audio.EnableCollisionNotification = true;
                audio.EnableGuidanceNotificaton = true;
                break;
            case 9: // Debug Scenario
                crane.ForceFeedback = true;
                audio.EnableCollisionNotification = true;
                audio.EnableGuidanceNotificaton = true;
                crane.JoystickControl = false;
                crane.speed = 30;
                break;
            default: 
                break;
        }
        crane.JoystickControl = false;
        crane.KeyboardControl = true;
        
        TargetContainers = 1; //shipstorage.Height * shipstorage.Width * shipstorage.Length;


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Writes user accuracy to file.
    /// Uses formula (1- (abs(avg/threshold))*100) to find out the relativistic accuracies of each user.
    /// This formula assumes a threshold of 40 units as being acceptable, but as near 0 as possible is better.
    /// </summary>
    /// <param name="Accuracies"></param>
    [Obsolete]
    public void WriteDataToFile(List<float> Accuracies, TimeSpan Time, GameObject StatisticsCanvas, GameObject ControlUI)
    {
        StatisticsCanvas.SetActive(true);
        ControlUI.SetActive(false);
        
        var threshold = 2;
        var avg = Accuracies.Sum(x => x) / Accuracies.Count;
        if (PlayerPrefs.GetInt("Scenario") == 3) { Guidance1 = true; Guidance2 = false; }
        else { Guidance2 = true; Guidance1 = false; }
        StreamWriter streamWriter = new StreamWriter(@"../Data/Data" + $"{PlayerPrefs.GetString("PlayerName")}" + $"{PlayerPrefs.GetInt("Scenario")}.txt");
        streamWriter.Write($"Scenario Selected: {PlayerPrefs.GetInt("Scenario")} (ForceFeedback: {crane.ForceFeedback}, GuidanceAudio1: {Guidance1}, GuidanceAudio2: {Guidance2})\n");
        streamWriter.Write($"Accuracy: (Worst: {Accuracies.Max(x => x),5:0.000}m, Best: {Accuracies.Min(x => x),5:0.000}m)\n Average Accuracy: {Accuracies.Average()}m\n");

        streamWriter.Write("All Accuracies:\n");
        foreach (var item in Accuracies.Select(x => x))
        {
            streamWriter.Write(item + "m\n");
        }
        print("Wrote to AccuracyData");

        StatisticsCanvas.transform.Find("Statistics").Find("StatisticsText").gameObject.GetComponent<TextMeshProUGUI>().text += $"\nAccuracy: (Worst: {Accuracies.Max(x => x),5:0.000}m, Best: {Accuracies.Min(x => x),5:0.000}m)\nAverage Accuracy: {Accuracies.Average()}m";

        streamWriter.Flush();
        streamWriter.WriteLine($"\nTime Elapsed for Task: {Time.Minutes} minutes, {Time.Seconds} Seconds");
        StatisticsCanvas.transform.Find("Statistics").Find("StatisticsText").gameObject.GetComponent<TextMeshProUGUI>().text += $"\nTime Elapsed for Task: {Time.Minutes} minutes, {Time.Seconds} Seconds";
        streamWriter.Flush();
        streamWriter.WriteLine("Collisions:");
        StatisticsCanvas.transform.Find("Statistics").Find("StatisticsText").gameObject.GetComponent<TextMeshProUGUI>().text += "\nCollisions: ";
        for (int i = 0; i < CollisionDetection.collisions.Count; i++)
        {
            string severity;
            switch (CollisionDetection.collisions[i])
            {
                case > 20000:
                    severity = "(SEVERE HIT):";
                    break;
                case > 10000:
                    severity = "(MODERATE HIT):";
                    break;
                default:
                    severity = "(LIGHT HIT):";
                    break;
            }
            StatisticsCanvas.transform.Find("Statistics").Find("StatisticsText").gameObject.GetComponent<TextMeshProUGUI>().text += $"\nCollision #{i:D2} {severity,-15} {CollisionDetection.collisions[i],5:0} kN";
            streamWriter.Write($"\nCollision #{i:D2} {severity,-15} {CollisionDetection.collisions[i],5:0} kN");
        }
        streamWriter.Close();
    }

    public void GoBackToMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
