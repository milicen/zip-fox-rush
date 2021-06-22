using Godot;
using System;

public class Game : Node
{
    [Signal] delegate void StartRunning();
    [Signal] delegate void ExitGame();
    [Signal] delegate void StopSpawn();
    [Export] PackedScene mainScene;
    Main main;

    Home home;

    [Export] PackedScene obstacleScene;
    Spawner spawner;

    Runner runner;
    Vector2 runnerPos;

    AudioStreamPlayer audio;
    AudioStreamPlayer audioDie;
    AudioStreamPlayer audioStart;
    public override void _Ready()
    {
        GD.Randomize();
        // main = mainScene.Instance() as Main;
        // main.Connect("ExitGame", this, "_on_ExitGame");

        home = GetNode<Home>("Home");
        home.Connect("StartGame", this, "_on_Home_StartGame");
        Connect("ExitGame", home, "_on_Main_ExitGame");

        runner = GetNode<Runner>("Runner");
        Connect("StartRunning", runner, "_on_StartRunning");

        runnerPos = runner.GlobalPosition;

        spawner = GetNode<Spawner>("Spawner");
        Connect("StopSpawn", spawner, "_on_StopSpawn");
        Connect("ExitGame", spawner, "_on_Game_ExitGame");

        audio = GetNode<AudioStreamPlayer>("SFX");
        audioDie = GetNode<AudioStreamPlayer>("SFX_Die");
        audioStart = GetNode<AudioStreamPlayer>("SFX_Start");

    }

    void _on_Home_StartGame() {
        audioStart.Play();
        InstanceMain();
        runner.GlobalPosition = runnerPos;
        runner.ReturnState();
    }

    void _on_ExitGame() {
        EmitSignal("ExitGame");
        // GetTree().CallGroup("obstacle", "queue_free");
        audio.Play();
        main.QueueFree();
        runner.GlobalPosition = runnerPos;
        runner.ReturnState();
    }

    void InstanceMain() {
        EmitSignal("StartRunning");

        main = mainScene.Instance() as Main;
        main.Connect("StartGame", spawner, "_on_StartGame");
        main.Connect("ButtonClick", this, "_on_ButtonClick");
        main.Connect("RunnerDie", this, "_on_RunnerDie");
        main.Connect("ExitGame", this, "_on_ExitGame");
        main.Connect("ReloadGame", this, "_on_Main_ReloadGame");
        main.Connect("GamePause", spawner, "_on_GamePause");
        main.Connect("GamePause", runner, "_on_GamePause");
        main.Connect("GameResume", spawner, "_on_GameResume");
        main.Connect("GameResume", runner, "_on_GameResume");
        AddChild(main);


        runner.Connect("HitObstacle", main, "_on_HitObstacle");
    }

    void _on_Main_ReloadGame() {
        main.QueueFree();
        InstanceMain();
        runner.GlobalPosition = runnerPos;
        runner.ReturnState();
    }

    void _on_RunnerDie() {
        audioDie.Play();
        EmitSignal("StopSpawn");
    }

    void _on_ButtonClick() {
        audio.Play();
    }
}
