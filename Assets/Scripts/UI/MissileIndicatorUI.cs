using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissileIndicatorUI : MonoBehaviour
{
    private NetworkCombatBase linkedPlane;

    [SerializeField] protected RectTransform indicator;
    [SerializeField] protected Image indicatorBackground;
    [SerializeField] protected Text indicatorText;

    [SerializeField] private DistanceCutoff[] warningCutoffs;

    [Header("Indicator placement")]
    [SerializeField] protected float edgeMargin = 10;

    [Header("Color Animation")]
    [SerializeField] protected Gradient colors;
    private float t = 0;
    private int dir = 1;

    private void OnValidate()
    {
        for (int i = 1; i < warningCutoffs.Length; i++)
        {
            if (warningCutoffs[i].percentDist < warningCutoffs[i - 1].percentDist)
            {
                Debug.LogError("Warning Cutoffs must be listed in ascending order", gameObject);
            }
        }
    }

    public void SetLinkedPlane(NetworkCombatBase plane)
    {
        linkedPlane = plane;
    }

    private void Update()
    {
        if (!linkedPlane)
        {
            indicator.gameObject.SetActive(false);
            return;
        }

        Transform target = GetClosestProjectile();

        indicator.gameObject.SetActive(target);
        if (!target)
        {
            t = 0;
            return;
        }

        Vector3 toTarget = (target.position - linkedPlane.transform.position);
        //SetIndicator(new Vector2(toTarget.x, toTarget.z).normalized);
        SetIndicator(Vector2.SignedAngle(new Vector2(Camera.main.transform.forward.x, Camera.main.transform.forward.z),
                                         new Vector2(toTarget.x, toTarget.z)));

        DistanceCutoff d = CalcCutoff(Vector3.Distance(target.position, linkedPlane.transform.position));
        UpdateIndicator(d.indicatorText);


        if (d.flashSpeed == 0)
        {
            AnimateIndicator(0);
        }
        else
        {
            t += Time.deltaTime * dir;
            t = Mathf.Clamp(t, 0, d.flashSpeed);
            if (t == 0)
                dir = 1;
            else if (t == d.flashSpeed)
                dir = -1;
            AnimateIndicator(t / d.flashSpeed);
        }
    }

    private Transform GetClosestProjectile()
    {
        Collider[] projectiles = Physics.OverlapSphere(linkedPlane.transform.position, linkedPlane.maxMissileDetectionRange, 1 << Constants.ProjectileLayer);

        if (projectiles.Length == 0)
            return null;

        float dist = Vector3.Distance(linkedPlane.transform.position, projectiles[0].transform.position);
        Collider closest = projectiles[0];
        for (int i = 1; i < projectiles.Length; i++)
        {
            float d = Vector3.Distance(linkedPlane.transform.position, projectiles[i].transform.position);
            if (d < dist)
            {
                dist = d;
                closest = projectiles[i];
            }
        }

        return closest.transform;
    }

    private DistanceCutoff CalcCutoff(float dist)
    {
        for (int i = 0; i < warningCutoffs.Length; i++)
        {
            if (dist < linkedPlane.maxMissileDetectionRange * warningCutoffs[i].percentDist)
            {
                return warningCutoffs[i];
            }
        }

        return new DistanceCutoff();
    }

    protected void SetIndicator(float ang)
    {
        Vector2 dir = Quaternion.AngleAxis(ang, Vector3.forward) * new Vector3(0, 1, 0);

        Vector2 pos = new Vector2(Screen.width, Screen.height) / 2;

        pos += new Vector2(dir.x * Screen.width / 2, dir.y * Screen.height / 2);

        pos -= dir * (edgeMargin + indicator.sizeDelta.x / 2);

        indicator.position = pos;
    }

    protected void UpdateIndicator(string text)
    {
        indicatorText.text = text;
    }

    protected void AnimateIndicator(float t)
    {
        indicatorBackground.color = colors.Evaluate(t);
    }

    [System.Serializable]
    private struct DistanceCutoff
    {
        public float percentDist;
        public string indicatorText;
        public float flashSpeed;
    }
}
