using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagramController : MonoBehaviour
{
    private int frame = 0;
    public DD_DataDiagram dd;
    private GameObject lineFood;
    private GameObject lineBacterium;
    // Start is called before the first frame update
    void Start()
    {
        AddLines();
    }

    private void AddLines()
    {
        lineFood = dd.AddLine("food", Color.blue);
        lineBacterium = dd.AddLine("bacterium", Color.green);
    }

    void FixedUpdate()
    {
        if (frame % 50 == 0)
        {
            dd.InputPoint(lineFood, new Vector2(1, GameObject.FindGameObjectsWithTag("food").Length));
            dd.InputPoint(lineBacterium, new Vector2(1, GameObject.FindGameObjectsWithTag("bacterium").Length));
        }
        frame++;
    }

    public void ClearDiagram()
    {
        dd.DestroyLine(lineFood);
        dd.DestroyLine(lineBacterium);
        AddLines();
    }
}
