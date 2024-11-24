using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }
    [SerializeField] private InputActionReference menuAction;

    [SerializeField] private GameObject menu;
    
    [SerializeField] private GameObject player;

    public bool isPaused = false;
    
    
    private void OnEnable()
    {
        menuAction.action.Enable();
    }

    private void OnDisable()
    {
        menuAction.action.Disable();
    }
    
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (menuAction.action.triggered)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    public void PauseGame()
    {
        isPaused = true;
        menu.SetActive(true);
    
        // Bloquer les entrées
        DisablePlayerControls();
    
        // Facultatif : désactiver l'interface utilisateur du jeu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        menu.SetActive(false);
    
        // Réactiver les entrées
        EnablePlayerControls();
    
        // Réactiver le curseur si nécessaire
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void DisablePlayerControls()
    {
        PlayerMovement movementController = player.GetComponent<PlayerMovement>();
        if (movementController != null)
            movementController.enabled = false;
        Shoot shootController = player.GetComponent<Shoot>();
        if (shootController != null)
            shootController.enabled = false;
        Sliding slideController = player.GetComponent<Sliding>();
        if (slideController != null)
            slideController.enabled = false;
        WallRunning wallRunController = player.GetComponent<WallRunning>();
        if (wallRunController != null)
            wallRunController.enabled = false;
        PlayerCamera cameraController = player.GetComponent<PlayerCamera>();
        if (cameraController != null)
            cameraController.enabled = false;
    }
    
    private void EnablePlayerControls()
    {
        PlayerMovement movementController = player.GetComponent<PlayerMovement>();
        if (movementController != null)
            movementController.enabled = true;
        Shoot shootController = player.GetComponent<Shoot>();
        if (shootController != null)
            shootController.enabled = true;
        Sliding slideController = player.GetComponent<Sliding>();
        if (slideController != null)
            slideController.enabled = true;
        WallRunning wallRunController = player.GetComponent<WallRunning>();
        if (wallRunController != null)
            wallRunController.enabled = true;
        PlayerCamera cameraController = player.GetComponent<PlayerCamera>();
        if (cameraController != null)
            cameraController.enabled = true;
    }
}
