using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NametagUI : MonoBehaviour
{
    private NetworkGamePlayer linkedPlayer;

    [SerializeField] private GameObject background;
    [SerializeField] private Text text;

    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);

    [Header("Scaling")]
    [SerializeField] private float minDist;
    [SerializeField] private float maxDist;
    [SerializeField] private float minScale;

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!linkedPlayer)
        {
            Destroy(gameObject);
            return;
        }

        background.SetActive(linkedPlayer.avatar);
        if (linkedPlayer.avatar)
        {
            rt.position = Camera.main.WorldToScreenPoint(linkedPlayer.avatar.transform.position + offset);

            float dist = Vector3.Distance(Camera.main.transform.position, linkedPlayer.avatar.transform.position);
            float t = dist > minDist ? 1 - dist / maxDist : 1;
            float scale = minScale + (1 - minScale) * t;
            rt.localScale = new Vector3(scale, scale, scale);
        }
    }

    public void SetLinkedPlayer(NetworkGamePlayer player)
    {
        linkedPlayer = player;

        text.text = player.displayName;
    }
}
