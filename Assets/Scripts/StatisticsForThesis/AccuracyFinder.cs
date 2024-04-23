using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AccuracyFinder : MonoBehaviour
{
    [Range(1,10)]public int TargetContainers;
    public CollisionDetection CollisionDetection;

    // Start is called before the first frame update
    void Start()
    {
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
    public void WriteDataToFile(List<float> Accuracies, TimeSpan Time)
    {
        var threshold = 20;
        var avg = Accuracies.Sum(x => x) / Accuracies.Count;
        StreamWriter streamWriter = new StreamWriter(@"../Data/Data" + $"{DateTime.Now:yyyyMMddHHmmss}.txt");
        var formula = (1 - avg / threshold) * 100;
        formula = Math.Max(0, Math.Min(100, formula));
        streamWriter.Write( formula + $"% Accuracy (Worst: {Accuracies.Max(x => x),5:0.000}m, Best: {Accuracies.Min(x => x),5:0.000}m) ");
        print("Wrote to AccuracyData");

        streamWriter.Flush();
        streamWriter.WriteLine($"\nTime Elapsed for Task: {Time.Minutes} minutes, {Time.Seconds} Seconds");
        streamWriter.Flush();
        streamWriter.WriteLine("Collisions:");
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
            streamWriter.Write($"\nCollision #{i:D2} {severity,-15} {CollisionDetection.collisions[i],5:0} kN");
        }
        streamWriter.Close();
    }
}
