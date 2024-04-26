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

    // Start is called before the first frame update
    void Start()
    {
        
        shipstorage = GameObject.Find("Ship").GetComponentInChildren<ContainerYardScript>();
        crane = GameObject.FindGameObjectWithTag("PanamaxCrane").GetComponent<ManualControlQuay>();
        audio = GameObject.FindGameObjectWithTag("PanamaxCrane").GetComponent<RayCastAroundCrane>();

        shipstorage.Height = 3;
        shipstorage.Width = 2;
        shipstorage.Length = 2;


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
            case 3: // Audio Guidance Only
                crane.ForceFeedback = false;
                audio.EnableCollisionNotification = false;
                audio.EnableGuidanceNotificaton = true;
                break;
            case 4: // Audio Collisions Only
                crane.ForceFeedback = false;
                audio.EnableCollisionNotification = true;
                audio.EnableGuidanceNotificaton = false;
                break;
            case 5: // Force Feedback and Guidance
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
                shipstorage.Height = 1;
                shipstorage.Width = 2;
                shipstorage.Length = 2;
                crane.speed = 30;
                break;
            default: 
                break;
        }
        TargetContainers = shipstorage.Height * shipstorage.Width * shipstorage.Length;

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
        StreamWriter streamWriter = new StreamWriter(@"../Data/Data" + $"{PlayerPrefs.GetString("PlayerName")}" + $"{PlayerPrefs.GetInt("Scenario")}.txt");
        var formula = (1 - avg / threshold) * 100;
        formula = Math.Max(0, Math.Min(100, formula));
        streamWriter.Write($"Scenario Selected: {PlayerPrefs.GetInt("Scenario")} (ForceFeedback: {crane.ForceFeedback}, CollisionAudio: {audio.EnableCollisionNotification}, GuidanceAudio: {audio.EnableGuidanceNotificaton})\n");
        streamWriter.Write( formula + $"% Accuracy (Worst: {Accuracies.Max(x => x),5:0.000}m, Best: {Accuracies.Min(x => x),5:0.000}m) ");
        print("Wrote to AccuracyData");

        StatisticsCanvas.transform.Find("Statistics").Find("StatisticsText").gameObject.GetComponent<TextMeshProUGUI>().text += "\n" + formula + $"% Accuracy (Worst: {Accuracies.Max(x => x),5:0.000}m, Best: {Accuracies.Min(x => x),5:0.000}m)";

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
