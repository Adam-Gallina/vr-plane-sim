using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NametagUI : MonoBehaviour
{
    [HideInInspector] public NetworkGamePlayer LinkedPlayer;

    [SerializeField] protected GameObject background;
    [SerializeField] protected Text playerNameText;

    [SerializeField] protected Vector3 offset = new Vector3(0, 2, 0);

    [Header("Scaling")]
    [SerializeField] protected float minDist;
    [SerializeField] protected float maxDist;
    [SerializeField] protected float hideDist;
    [SerializeField] protected float minScale;

    private RectTransform rt;

    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    public virtual void SetLinkedPlayer(NetworkGamePlayer player)
    {
        LinkedPlayer = player;

        playerNameText.text = player.displayName;
    }

    protected virtual void Update()
    {
        if (!LinkedPlayer)
        {
            Destroy(gameObject);
            return;
        }

        background.SetActive(LinkedPlayer.avatar && CanSeeTarget() && SetRectTransform());
    }

    protected bool CanSeeTarget()
    {
        if (Physics.Linecast(Camera.main.transform.position, LinkedPlayer.avatar.transform.position, 1 << Constants.EnvironmentLayer))
            return false;

        return Vector3.Angle(Camera.main.transform.forward, LinkedPlayer.avatar.transform.position - Camera.main.transform.position) < Camera.main.fieldOfView;
    }

    protected virtual bool SetRectTransform()
    {
        float dist = Vector3.Distance(Camera.main.transform.position, LinkedPlayer.avatar.transform.position);
        if (dist > maxDist)
            return false;

        rt.position = Camera.main.WorldToScreenPoint(LinkedPlayer.avatar.transform.position + offset);

        float scale = minScale + (1 - minScale) * CalcT(dist);
        rt.localScale = new Vector3(scale, scale, scale);

        return true;
    }

    protected float CalcT(float dist)
    {
        if (dist < minDist)
            return 1;
        else if (dist >= maxDist)
            return 0;

        return 1 - (dist / maxDist);
    }
}
