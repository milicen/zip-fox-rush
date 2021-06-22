using Godot;
using System;

public class Obstacle : Area2D
{
    [Signal] delegate void ExitScreen(Obstacle obstacle);
    [Export] float speed = 200f;
    float currentSpeed = 0;
    [Export] Vector2 waitingPos;


    public override void _Ready()
    {
        
    }

    public void Init(Vector2 position) {
        GlobalPosition = position;
        currentSpeed = speed;
    }

    public void ReturnState() {
        GlobalPosition = waitingPos;
    }

    public override void _Process(float delta)
    {
        Position += Vector2.Down * currentSpeed * delta;
    }

    void _on_VisibilityNotifier2D_viewport_exited(Viewport vp) {
        currentSpeed = 0;
        EmitSignal("ExitScreen", this);
    }

    void _on_StopMove() {
        currentSpeed = 0;
    }

    void _on_StartMove() {
        currentSpeed = speed;
    }
}
