using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticDataManager : MonoBehaviour
{
    private static StaticDataManager _instance;
    public static StaticDataManager Instance { get { return _instance; } }

    public List<Vector3?> ContainersLoaded = new List<Vector3?>();
    public List<Vector3Int> pointersTakeablestatic;
    public List<Vector3Int> pointersPlaceablestatic;


    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
