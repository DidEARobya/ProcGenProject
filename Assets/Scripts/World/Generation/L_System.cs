using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public struct TransformData
{
    public Vector3 position;
    public Quaternion rotation;
}
public class L_System : MonoBehaviour
{
    [SerializeField]
    public GameObject turtlePrefab;

    private GameObject turtle;

    private GameObject treeSystem;
    private GameObject sierpinkskiSystem;
    private GameObject dragonCurveSystem;

    [SerializeField]
    public float lineLength;
    [SerializeField]
    public int iterations;

    [SerializeField]
    public Material lineMaterial;

    private float lineAngle;
    private string axiom;

    private LineRenderer lineRenderer;

    protected Dictionary<char, string> recursionRules;
    Stack<TransformData> stack;

    private List<GameObject> lsystems;

    private string modelString;

    // Start is called before the first frame update
    void Start()
    {
        lsystems = new List<GameObject>();

        treeSystem = GenerateRules("Plant");
        treeSystem.SetActive(false);
        treeSystem.name = "Tree";
        lsystems.Add(treeSystem);

        sierpinkskiSystem = GenerateRules("Sierpinkski");
        sierpinkskiSystem.SetActive(false);
        sierpinkskiSystem.name = "Sierpinkski";
        lsystems.Add(sierpinkskiSystem);

        dragonCurveSystem = GenerateRules("DragonCurve");
        dragonCurveSystem.SetActive(false);
        dragonCurveSystem.name = "Dragon Curve";
        lsystems.Add(dragonCurveSystem);
    }

    public void SpawnLSystem(Vector3Int position)
    {
        int rand = Random.Range(0, lsystems.Count);

        GameObject lsystem = Instantiate(lsystems[rand], position, Quaternion.identity);
        lsystem.SetActive(true);
        lsystem.name = lsystems[rand].name;

        Destroy(lsystem, 5f);
    }

    private void ResetVariables()
    {
        turtle = Instantiate(turtlePrefab, turtlePrefab.transform);

        if(turtle.GetComponent<LineRenderer>() != null)
        {
            lineRenderer = turtle.GetComponent<LineRenderer>();
        }
        else
        {
            lineRenderer = turtle.AddComponent<LineRenderer>();
        }

        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = 0;
        lineRenderer.material = lineMaterial;

        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;

        if (stack == null)
        {
            stack = new Stack<TransformData>();
        }

        stack.Clear();

        turtle.transform.position = Vector3.zero;
        turtle.transform.rotation = Quaternion.identity;

        if (recursionRules == null)
        {
            recursionRules = new Dictionary<char, string>();
        }

        recursionRules.Clear();
    }
    private GameObject GenerateRules(string ruleSet)
    {
        ResetVariables();

        switch(ruleSet)
        {
            case "Plant":

                modelString = ruleSet;
                recursionRules.Add('X', "F+[[X]-X]-F[-FX]+X");
                recursionRules.Add('F', "FF");

                lineAngle = 25;
                axiom = "X";
                break;
            case "Sierpinkski":
                modelString = ruleSet;
                recursionRules.Add('F', "F-G+F+G-F");
                recursionRules.Add('G', "GG");

                lineAngle = 120;
                axiom = "F-G-G";
                break;
            case "DragonCurve":
                modelString = ruleSet;
                recursionRules.Add('F', "F+G");
                recursionRules.Add('G', "F-G");

                lineAngle = 90;
                axiom = "F";
                break;
        }

        return GenerateString();
    }

    private GameObject GenerateString()
    {
        string temp = axiom;
        StringBuilder stringBuilder = new StringBuilder();

        for(int i = 0; i < iterations; i++)
        {
            foreach(char c in temp)
            {
                if(recursionRules.ContainsKey(c))
                {
                    stringBuilder.Append(recursionRules[c]);
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }

            temp = stringBuilder.ToString();
            stringBuilder = new StringBuilder();
        }

        return ApplyRules(temp);
    }

    
    #region MyCode
    private GameObject ApplyRules(string str)
    {
        int lrIndex = 0;

        switch(modelString)
        {
            case "Plant":

                foreach (char c in str)
                {
                    switch (c)
                    {
                        case 'X':
                            break;

                        case 'F':
                            Vector3 lastPosition = turtle.transform.position;
                            turtle.transform.Translate(Vector3.up * lineLength);

                            lineRenderer.positionCount += 2;
                            lineRenderer.SetPosition(lrIndex, lastPosition);
                            lineRenderer.SetPosition(lrIndex + 1, turtle.transform.position);

                            lrIndex++;
                            break;

                        case '+':
                            turtle.transform.Rotate(new Vector3(0, 0, lineAngle));
                            break;

                        case '-':
                            turtle.transform.Rotate(new Vector3(0, 0, -lineAngle));
                            break;

                        case '[':
                            TransformData turtlePos = new TransformData();
                            turtlePos.position = turtle.transform.position;
                            turtlePos.rotation = turtle.transform.rotation;

                            stack.Push(turtlePos);
                            break;

                        case ']':
                            TransformData lastPos = stack.Pop();

                            turtle.transform.position = lastPos.position;
                            turtle.transform.rotation = lastPos.rotation;
                            break;

                    }
                }

                break;
            case "Sierpinkski":

                foreach (char c in str)
                {
                    switch (c)
                    {
                        case 'F':
                            Vector3 lastPosition = turtle.transform.position;
                            turtle.transform.Translate(Vector3.up * lineLength);

                            lineRenderer.positionCount += 2;
                            lineRenderer.SetPosition(lrIndex, lastPosition);
                            lineRenderer.SetPosition(lrIndex + 1, turtle.transform.position);

                            lrIndex++;
                            break;

                        case 'G':
                            Vector3 lastPos = turtle.transform.position;
                            turtle.transform.Translate(Vector3.up * lineLength);

                            lineRenderer.positionCount += 2;
                            lineRenderer.SetPosition(lrIndex, lastPos);
                            lineRenderer.SetPosition(lrIndex + 1, turtle.transform.position);

                            lrIndex++;
                            break;

                        case '+':
                            turtle.transform.Rotate(new Vector3(0, 0, -lineAngle));
                            break;

                        case '-':
                            turtle.transform.Rotate(new Vector3(0, 0, lineAngle));
                            break;

                    }
                }

                break;
            case "DragonCurve":

                foreach (char c in str)
                {
                    switch (c)
                    {
                        case 'F':
                            Vector3 lastPosition = turtle.transform.position;
                            turtle.transform.Translate(Vector3.up * lineLength);

                            lineRenderer.positionCount += 2;
                            lineRenderer.SetPosition(lrIndex, lastPosition);
                            lineRenderer.SetPosition(lrIndex + 1, turtle.transform.position);

                            lrIndex++;
                            break;

                        case 'G':
                            Vector3 lastPos = turtle.transform.position;
                            turtle.transform.Translate(Vector3.up * lineLength);

                            lineRenderer.positionCount += 2;
                            lineRenderer.SetPosition(lrIndex, lastPos);
                            lineRenderer.SetPosition(lrIndex + 1, turtle.transform.position);

                            lrIndex++;
                            break;

                        case '+':
                            turtle.transform.Rotate(new Vector3(0, 0, -lineAngle));
                            break;

                        case '-':
                            turtle.transform.Rotate(new Vector3(0, 0, lineAngle));
                            break;

                    }
                }

                break;
        }

        return turtle;
    }
#endregion
}
