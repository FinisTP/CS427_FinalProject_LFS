using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forcefield : MonoBehaviour
{
    private float currentTime;
    private float maxTime;
    private void Start()
    {
        BattleRoyaleController brc = FindObjectOfType<BattleRoyaleController>();
        maxTime = brc.matchTime;
        currentTime = 0;
    }
    private void Update()
    {
        currentTime += Time.deltaTime;
        transform.localScale = Vector3.one * 1000 * ((maxTime-currentTime)/maxTime);
        if (currentTime >= maxTime)
        {
            // GameplayManager.instance.WinGame();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Vector3.Distance(transform.position, other.transform.position) <= transform.localScale.x) return;
            ThirdPersonMovement tpm = other.GetComponent<ThirdPersonMovement>();
            if (!tpm.isDefeated)
            {
                tpm.SetDefeated(500f);
            }
        }
    }
}
