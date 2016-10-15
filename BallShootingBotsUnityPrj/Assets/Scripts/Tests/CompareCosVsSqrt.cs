using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CompareCosVsSqrt : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        List<float> rndNumbs = new List<float>();
        const int TEST_VALUE_COUNT = 1000000;
        for (int i = 0; i < TEST_VALUE_COUNT; i++)
        {
            rndNumbs.Add(Random.Range(0, 1.0f));
        }
        float[] arrayOfVals = new float[rndNumbs.Count];
        float startTimestamp = Time.realtimeSinceStartup;
        for(int i = 0; i < rndNumbs.Count; i++)
        {
            float val = Mathf.Cos(rndNumbs[i]);
            arrayOfVals[i] = val;
        }
        float elapsedTime = Time.realtimeSinceStartup - startTimestamp;
        Debug.Log("Cosine result " + elapsedTime);

        startTimestamp = Time.realtimeSinceStartup;
        for (int i = 0; i < rndNumbs.Count; i++)
        {
            float val = Mathf.Sqrt(rndNumbs[i]);
            arrayOfVals[i] = val;
        }
        elapsedTime = Time.realtimeSinceStartup - startTimestamp;
        Debug.Log("Sqrt result " + elapsedTime);
    }

    // Update is called once per frame
    void Update ()
    {
	
	}
}
