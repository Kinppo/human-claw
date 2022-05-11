using DG.Tweening;
using UnityEngine;

public class Reward : MonoBehaviour
{
    private bool _isTriggered;
    public ParticleSystem collectEffect;
    public new Renderer renderer;

    private void OnTriggerEnter(Collider other)
    {
        if (_isTriggered || other.gameObject.layer != 6) return;

        _isTriggered = true;
        transform.DOScale(0.25f, 0.1f).OnComplete(() =>
        {
            renderer.enabled = false;
            collectEffect.gameObject.SetActive(true);
            UIManager.Instance.UpdateReward();
            AudioManager.Instance.PlaySound(AudioManager.Instance.gemCollect);
            UIManager.Instance.UpdateSpeedSlider();
            Destroy(gameObject, 3f);
        });
    }
}