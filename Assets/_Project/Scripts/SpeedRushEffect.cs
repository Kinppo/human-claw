using PathCreation;
using UnityEngine;

public class SpeedRushEffect : MonoBehaviour
{
    public float dstTravelled;

    void Start()
    {
        Destroy(gameObject, 2f);
    }

    private void Update()
    {
        transform.position = Player.Instance.path.path.GetPointAtDistance(dstTravelled, EndOfPathInstruction.Stop) +
                             new Vector3(0, -0.75f, 0);
        transform.rotation = Player.Instance.path.path.GetRotationAtDistance(dstTravelled, EndOfPathInstruction.Stop);
    }
}