using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using NiEngine;

[NotSaved]
public class Billboard : MonoBehaviour
{
    public bool AutoTargetMainCamera = true;
    public GameObject Target;

    public enum LockAxisEnum
    {
        None,
        X,
        Y,
    }
    [Tooltip("Only rotate around this axis")]
    public LockAxisEnum LockAxis = LockAxisEnum.Y;


    //[Tooltip("If LockAxis is set to 'OtherY'. Will lock on the Y axis of this other transform object.")]
    //public Transform Other;

    // Start is called before the first frame update
    void Start()
    {
        if (AutoTargetMainCamera)
        {
            Target = Camera.main.gameObject;
        }   
    }

    void Update()
    {
        LookAt();
    }

    public void SetTarget(GameObject gameObject)
    {
        Target = gameObject;
        LookAt();
    }

    void LookAt()
    {
        float3 target = Target.transform.position;
        float3 here = transform.position;
        var diff = target - here;
        switch (LockAxis)
        {
            case LockAxisEnum.X:
                {
                    diff.x = 0;
                    var dir = math.normalize(diff);
                    var up = math.cross(dir, new float3(1, 0, 0));
                    var upL = math.length(up);
                    if (upL < 0.001) return;
                    up = up * math.rcp(upL);
                    transform.rotation = quaternion.LookRotation(dir, up);
                    break;
                }
            case LockAxisEnum.Y:
                {
                    diff.y = 0;
                    var dir4Y = math.normalize(diff);
                    transform.rotation = quaternion.LookRotation(dir4Y, new float3(0, 1, 0));
                    return;
                }
            case LockAxisEnum.None:
                {
                    var dir4Y = math.normalize(diff);
                    transform.rotation = quaternion.LookRotationSafe(dir4Y, new float3(0, 1, 0));
                    return;
                }
        }
    }
}
