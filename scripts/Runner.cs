using Godot;
using System;

public class Runner : Node2D
{
    [Signal] delegate void HitObstacle();

    Area2D redFox;
    Area2D blueFox;

    AnimatedSprite soulSprite;
    [Export] Vector2 soulInitialScale;
    [Export] Vector2 soulFinalScale;

    Vector2 redInitialPos;
    Vector2 blueInitialPos;

    Vector2 groupPosition;
    Vector2 redFoxPosition;
    Vector2 blueFoxPosition;

    [Export] int maxDistance = 190;
    [Export] int minDistance = 64;
    [Export] int gap = 64;

    [Export] int minXPos = 256;
    [Export] int maxXPos = 512;

    bool isOpen = false;
    bool canMove = true;
    bool isStopped = false; 

    bool startRunning = false;

    Tween movementTween;
    Tween zipTween;

    AudioStreamPlayer audio;

    public override void _Ready()
    {
        redFox = GetNode<Area2D>("RedFox");
        blueFox = GetNode<Area2D>("BlueFox");

        soulSprite = GetNode<AnimatedSprite>("SoulSprite");

        movementTween = GetNode<Tween>("MovementTween");
        zipTween = GetNode<Tween>("ZipTween");

        groupPosition = Position;
        redInitialPos = redFox.Position;
        blueInitialPos = blueFox.Position;

        audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        ReturnState();
    }

    public override void _Process(float delta)
    {
        if(startRunning && !isStopped) {
            HandleMovement();
            HandleZipper(delta);
        }
    }

    void HandleMovement() {
        if(Input.IsActionJustPressed("left") && canMove) {
            if((isOpen && GlobalPosition.x > minXPos + gap) || (!isOpen && GlobalPosition.x > minXPos)) {
                canMove = false;
                groupPosition = Position + Vector2.Left * gap;
                // Position += Vector2.Left * gap;
                movementTween.InterpolateProperty(this, "position", Position, groupPosition, .1f, Tween.TransitionType.Sine);
                movementTween.Start();
            }
        }
        else if(Input.IsActionJustPressed("right") && canMove) {
            if((isOpen && GlobalPosition.x < maxXPos - gap) || (!isOpen && GlobalPosition.x < maxXPos)) {
                canMove = false;
                groupPosition = Position + Vector2.Right * gap;
                // Position += Vector2.Right * gap;
                movementTween.InterpolateProperty(this, "position", Position, groupPosition, .1f);
                movementTween.Start();
            }
        }

        if((groupPosition - Position).Length() < 1f) {
            Position = groupPosition;
        }

        // GD.Print(Position);
    }

    async void HandleZipper(float delta) {
        var distance = (blueFox.Position - redFox.Position).Length();
        if(Input.IsActionPressed("zipper_up") && distance < maxDistance) {
            isOpen = true;
            if(GlobalPosition.x >= maxXPos) {
                groupPosition = Position + Vector2.Left * gap;
                movementTween.InterpolateProperty(this, "position", Position, groupPosition, .1f);
                movementTween.Start();
            }
            else if(GlobalPosition.x <= minXPos) {
                groupPosition = Position + Vector2.Right * gap;
                movementTween.InterpolateProperty(this, "position", Position, groupPosition, .1f);
                movementTween.Start();

            }
            redFoxPosition = redFox.Position + Vector2.Left * gap;
            blueFoxPosition = blueFox.Position + Vector2.Right * gap;

            zipTween.InterpolateProperty(soulSprite, "scale", soulInitialScale, soulFinalScale, .01f);
            zipTween.InterpolateProperty(redFox, "position", redFox.Position, redFoxPosition, .1f);
            zipTween.InterpolateProperty(blueFox, "position", blueFox.Position, blueFoxPosition, .1f);

            if(!zipTween.IsActive()) {
                zipTween.Start();
            }
        }

        if(distance > maxDistance) {
            redFox.Position = Vector2.Left * gap + Vector2.Left * 32;
            blueFox.Position = Vector2.Right * gap + Vector2.Right * 32;
        }

        if(Input.IsActionJustReleased("zipper_up") && distance > minDistance) {
            redFoxPosition = redFox.Position + Vector2.Right * gap;
            blueFoxPosition = blueFox.Position + Vector2.Left * gap;

            zipTween.InterpolateProperty(redFox, "position", redFox.Position, redFoxPosition, .1f);
            zipTween.InterpolateProperty(blueFox, "position", blueFox.Position, blueFoxPosition, .1f);
            zipTween.InterpolateProperty(soulSprite, "scale", soulFinalScale, soulInitialScale, .1f);

            zipTween.Start();

            await ToSignal(zipTween, "tween_all_completed");

            isOpen = false;
        }

        if(distance < minDistance) {
            ReturnState();
            // isOpen = false;
        }
    }

    void _on_RedFox_area_entered(Area2D body) {
        if(body.IsInGroup("obstacle")) {
            EmitSignal("HitObstacle");
            startRunning = false;
            isStopped = true;

            audio.Play();

            ReturnState();
            // play 'Poof' animation
        }
    }

    void _on_BlueFox_area_entered(Area2D body) {
        if(body.IsInGroup("obstacle")) {
            EmitSignal("HitObstacle");
            startRunning = false;
            isStopped = true;

            audio.Play();

            ReturnState();

            // play 'Poof' animation
        }
    }

    void _on_MovementTween_tween_completed(Tween tween, NodePath path) {
        canMove = true;
    }

    void _on_StartRunning() {
        startRunning = true;
        isStopped = false;
    }

    public void ReturnState() {
        redFox.Position = Vector2.Left * 32;
        blueFox.Position = Vector2.Right * 32;
        soulSprite.Scale = soulInitialScale;
        isOpen = false;
    }

    void _on_GamePause() {
        isStopped = true;
    }

    void _on_GameResume() {
        isStopped = false;
    }


}
