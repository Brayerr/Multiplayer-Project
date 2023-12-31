using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Explosion : MonoBehaviourPunCallbacks
{
    public string UPDATE_EXPLOSION_ACTOR_NUM = nameof(UpdateActorNum);

    [SerializeField] List<GameObject> VFXs;

    Transform pickedVfxTransform;

    GameObject vfxGameObject;
    public int actorNum;
    string vfxName;
    // Start is called before the first frame update
    void Start()
    {
        vfxGameObject = VFXs[Random.Range(0, VFXs.Count)];
        vfxName = vfxGameObject.name;
        vfxGameObject = PhotonNetwork.Instantiate("VFX/" + vfxName, transform.position, transform.rotation);
        pickedVfxTransform = vfxGameObject.transform;
        Destroy(gameObject, 5f);


        Explode(transform.position);
    }



    void Explode(Vector3 hitPoint)
    {
        Grow();
        Collider[] hitColliders = Physics.OverlapSphere(hitPoint, 5);
        foreach (var hitCollider in hitColliders)
        {
            hitCollider.attachedRigidbody?.AddForce((hitPoint - hitCollider.transform.position).normalized * -20, ForceMode.Impulse);
            if (hitCollider.gameObject.TryGetComponent(out PlayerController conroller))
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    conroller.photonView.RPC(conroller.UPDATE_PLAYER_ACTOR_HIT, RpcTarget.All, actorNum);
                }
                //conroller.lastActorHit = actorNum;
                print($"player last hit by actor number {conroller.lastActorHit}");
            }
        }
    }

    void Grow()
    {
        Tween tween = pickedVfxTransform.DOScale(vfxGameObject.transform.localScale * 5, 1);
        tween.OnComplete(() =>
        {
            tween.Kill();
            Invoke("Destroye", 1f);
        });
    }

    void Destroye()
    {
        Destroy(gameObject);
    }

    [PunRPC]
    public void UpdateActorNum(int newAcNum)
    {
        actorNum = newAcNum;
    }
}
