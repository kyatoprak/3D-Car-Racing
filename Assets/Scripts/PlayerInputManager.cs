using UnityEngine;

public class PlayerInputManager : MonoBehaviour, IInputProvider
{
    public float VerticalInput { get; private set; }
    public float HorizontalInput { get; private set; }
    public bool HandbrakeInput { get; private set; }
    public bool EscapePressed { get; private set; } 
    private bool _pauseToggleState = false;

    private void FixedUpdate()
    {
        VerticalInput = Input.GetAxis("Vertical");
        HorizontalInput = Input.GetAxis("Horizontal");
        HandbrakeInput = Input.GetAxis("Jump") != 0;
       
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _pauseToggleState = !_pauseToggleState;
        }

        EscapePressed = _pauseToggleState;
    }
}