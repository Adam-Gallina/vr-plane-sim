using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VrGameNametag : NametagUI
{
    protected override void Update()
    {
        base.Update();

        transform.LookAt(Camera.main.transform);
    }

    protected override bool SetRectTransform()
    {
        float dist = Vector3.Distance(Camera.main.transform.position, LinkedPlayer.avatar.transform.position);
        if (dist > maxDist)
            return false;

        transform.position = LinkedPlayer.avatar.transform.position + offset;

        float scale = minScale + (1 - minScale) * CalcT(dist);
        transform.localScale = new Vector3(scale, scale, scale);

        return true;
    }
}
