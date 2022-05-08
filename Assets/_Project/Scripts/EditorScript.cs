using PathCreation;
using UnityEngine;

[ExecuteInEditMode]
public class EditorScript : MonoBehaviour
{
    public bool isTriggered;
    public float distanceTravelled;
    private PathCreator path;
    private Vector3 initialRotation;
    
    void Update()
    {
        //if (isTriggered) return;
        var level = PlayerPrefs.GetInt("level", 1);
        var selectedLevel = FindObjectOfType<GameManager>().levels[level - 1];
        path = selectedLevel.path;
        initialRotation = transform.eulerAngles;
     
        
        transform.position = new Vector3(path.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop).x,
            transform.position.y, path.path.GetPointAtDistance(distanceTravelled, EndOfPathInstruction.Stop).z);
        
        transform.rotation = path.path.GetRotationAtDistance(distanceTravelled, EndOfPathInstruction.Stop);
        transform.eulerAngles += new Vector3(0, 0, initialRotation.z);
    
        isTriggered = true;
    }
}