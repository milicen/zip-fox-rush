using Godot;
using System;
using System.Collections.Generic;

public class Main : Node
{
    [Signal] delegate void StartGame();
    [Signal] delegate void ExitGame();
    [Signal] delegate void ReloadGame();
    [Signal] delegate void GamePause();
    [Signal] delegate void GameResume();

    [Signal] delegate void RunnerDie();
    [Signal] delegate void ButtonClick();

    [Export] Vector2 gameoverPanelStartPos;
    [Export] Vector2 gameoverPanelFinalPos;

    Timer countdownTimer;
    [Export] float countdown = 4f;
    float score = 0;
    bool startCount = false;

    Label countdownLabel;
    Panel panel;
    NinePatchRect gameoverPanel;
    Button resumeButton;
    TextureButton pauseButton;
    Label scoreLabel;

    Spawner spawner;
    Runner runner;

    Tween tween;

    AudioStreamPlayer audio;

    public override void _Ready()
    {
        countdownTimer = GetNode<Timer>("CountdownTimer");
        countdownTimer.WaitTime = countdown;

        countdownLabel = GetNode<Label>("CanvasLayer/CountdownLabel");

        panel = GetNode<Panel>("CanvasLayer/Panel");
        panel.Hide();

        gameoverPanel = GetNode<NinePatchRect>("CanvasLayer/GameoverPanel");
        gameoverPanel.Hide();

        resumeButton = GetNode<Button>("CanvasLayer/ResumeButton");
        resumeButton.Disabled = true;
        resumeButton.Hide();

        pauseButton = GetNode<TextureButton>("CanvasLayer/PauseButton");
        pauseButton.Disabled = true;
        pauseButton.Hide();

        scoreLabel = GetNode<Label>("CanvasLayer/Control/ScoreContainer/ScoreLabel");

        // spawner = GetNode<Spawner>("Spawner");
        // Connect("StartGame", spawner, "_on_StartGame");

        tween = GetNode<Tween>("CanvasLayer/Tween");

        audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        countdownTimer.Start();

    }

    public override void _Process(float delta)
    {
        if(countdownTimer.TimeLeft < 1f) {
            countdownLabel.Text = "Start!";
        }
        else {
            countdownLabel.Text = Mathf.Floor(countdownTimer.TimeLeft).ToString();
        }
        
        if(startCount) {
            score += delta;
        }

        scoreLabel.Text = Math.Round(score, 1).ToString();
    }


    void _on_CountdownTimer_timeout() {
        startCount = true;

        panel.Hide();
        countdownLabel.Hide();

        pauseButton.Disabled = false;
        pauseButton.Show();

        EmitSignal("StartGame");
    }

    void _on_HitObstacle() {   
        pauseButton.Disabled = true;
        pauseButton.Hide();
        EmitSignal("RunnerDie");
        panel.Show();

        gameoverPanel.GetNode<Label>("MarginContainer/VBoxContainer/Gameover").Text = "You've reached " + Math.Round(score, 1).ToString() + " miles!";
        gameoverPanel.Show();

        GetTree().Paused = true;

        tween.InterpolateProperty(gameoverPanel, "rect_position", gameoverPanelStartPos, gameoverPanelFinalPos, 1.5f, Tween.TransitionType.Bounce, Tween.EaseType.Out);
        tween.Start();


    }

    async void _on_Retry_pressed() {
        EmitSignal("ButtonClick");

        // GetTree().CallGroup("obstacle", "queue_free");

        tween.InterpolateProperty(gameoverPanel, "rect_position", gameoverPanelFinalPos, gameoverPanelStartPos, .8f, Tween.TransitionType.Bounce, Tween.EaseType.Out);
        tween.Start();

        await ToSignal(tween, "tween_completed");
        
        EmitSignal("ReloadGame");

        GetTree().Paused = false;
    }

    void _on_PauseButton_pressed() {

        EmitSignal("ButtonClick");
        EmitSignal("GamePause");

        panel.Show();
        resumeButton.Disabled = false;
        resumeButton.Show();
        GetTree().Paused = true;

    }

    void _on_ResumeButton_pressed() {

        EmitSignal("ButtonClick");
        EmitSignal("GameResume");


        panel.Hide();
        resumeButton.Disabled = true;
        resumeButton.Hide();
        GetTree().Paused = false;
    }

    void _on_Home_pressed() {
        GetTree().Paused = false;
        EmitSignal("ExitGame");
    }
}
