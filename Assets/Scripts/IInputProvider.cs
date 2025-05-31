using UnityEngine;

public interface IInputProvider
{
    float VerticalInput { get; }
    float HorizontalInput { get; }
    bool HandbrakeInput { get; }
}
