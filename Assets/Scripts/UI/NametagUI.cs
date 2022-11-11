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
    [SerializeField] private float hideDist;
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

        background.SetActive(linkedPlayer.avatar && CanSeeTarget() && SetRectTransform());
    }

    private bool CanSeeTarget()
    {
        if (Physics.Linecast(Camera.main.transform.position, linkedPlayer.avatar.transform.position, 1 << Constants.EnvironmentLayer))
            return false;

        return Vector3.Angle(Camera.main.transform.forward, linkedPlayer.avatar.transform.position - Camera.main.transform.position) < Camera.main.fieldOfView;
    }

    private bool SetRectTransform()
    {
        float dist = Vector3.Distance(Camera.main.transform.position, linkedPlayer.avatar.transform.position);
        if (dist > maxDist)
            return false;

        rt.position = Camera.main.WorldToScreenPoint(linkedPlayer.avatar.transform.position + offset);

        float scale = minScale + (1 - minScale) * CalcT(dist);
        rt.localScale = new Vector3(scale, scale, scale);

        return true;
    }

    private float CalcT(float dist)
    {
        if (dist < minDist)
            return 1;
        else if (dist >= maxDist)
            return 0;

        return 1 - (dist / maxDist);
    }

    public void SetLinkedPlayer(NetworkGamePlayer player)
    {
        linkedPlayer = player;

        text.text = player.displayName;
    }

}
