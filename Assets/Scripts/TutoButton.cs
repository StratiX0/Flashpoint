using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutoButton : MonoBehaviour
{
    [SerializeField] private InputActionReference resetAction;
    [SerializeField] private float resetDistance;
    
    [SerializeField] private List<TargetHealth> targets;

    private void OnEnable()
    {
        resetAction.action.Enable();
    }

    private void OnDisable()
    {
        resetAction.action.Disable();
    }

    void Update()
    {
        if (MenuManager.Instance.isPaused) return;
        if (resetAction.action.triggered)
        {
            ResetTargetsHealth();
        }
    }

    private void ResetTargetsHealth()
    {
        foreach (var target in targets)
        {
            if (target != null)
            {
                target.SetHealth(target.defaultHealth);
                target.SetMaterial(target.defaultMat);
            }
        }
    }
}
