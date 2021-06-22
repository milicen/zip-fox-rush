using Godot;
using System;

public class Home : Node
{
    [Signal] delegate void StartGame();

    Button button;
    AnimationPlayer anim;

    public override void _Ready()
    {
        button = GetNode<Button>("CanvasLayer/MarginContainer/VBoxContainer/Button");
        anim = GetNode<AnimationPlayer>("AnimationPlayer");
        anim.Play("text blink");

    }

    void _on_Button_pressed() {
        EmitSignal("StartGame");
        button.Disabled = true;
        GetNode<MarginContainer>("CanvasLayer/MarginContainer").Hide();
    }

    void _on_Main_ExitGame() {
        button.Disabled = false;
        GetNode<MarginContainer>("CanvasLayer/MarginContainer").Show();
    }
}
