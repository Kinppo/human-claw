using System.Collections;
using UnityEngine;

public class ObstaclePart : MonoBehaviour
{
    public Rigidbody rb;
    public Obstacle obstacleParent;
    private bool _isTriggered;

    private void OnTriggerEnter(Collider collision)
    {
        if (_isTriggered || collision.gameObject.layer != 6) return;

        _isTriggered = true;
        if (GameManager.gameState == GameState.SpeedRush)
            rb.isKinematic = false;
        else if (obstacleParent.obstacleType == ObstacleType.Yellow)
        {
            StartCoroutine(DropObstacle());
            Player.Instance.DropObject();
        }
        else if (obstacleParent.obstacleType == ObstacleType.Red)
            Player.Instance.Fall();
    }

    private IEnumerator DropObstacle()
    {
        var time = Random.Range(0, 0.15f);
        yield return new WaitForSeconds(time);
        rb.isKinematic = false;
        rb.AddForce(new Vector3(Random.Range(-70, 70),Random.Range(-50, 50),Random.Range(-70, 70)));
        Destroy(gameObject, 3f);
    }
}