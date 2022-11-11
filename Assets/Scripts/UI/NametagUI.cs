using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NametagUI : MonoBehaviour
{
    private NetworkGamePlayer linkedPlayer;

    [SerializeField] private Text text;

    [SerializeField] private Vector3 offset = new Vector3(0, 2, 0);

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!linkedPlayer)
            Destroy(gameObject);

        gameObject.SetActive(linkedPlayer.avatar);
        rt.localPosition = Camera.main.WorldToScreenPoint(linkedPlayer.avatar.transform.position + offset);
    }

    public void SetLinkedPlayer(NetworkGamePlayer player)
    {
        linkedPlayer = player;

        text.text = player.displayName;
    }
}
