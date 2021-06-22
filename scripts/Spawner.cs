using Godot;
using System;
using System.Collections.Generic;

public class Spawner : Node2D
{
    [Signal] delegate void StopMove();
    [Signal] delegate void StartMove();
    [Export] PackedScene obstacleScene;

    Godot.Collections.Array<Position2D> spawnPoints = new Godot.Collections.Array<Position2D>();

    int[,] valueArray = {
        {0, 0, 1, 1, 1, 1},
        {1, 0, 0, 1, 1, 1},
        {1, 1, 0, 0, 1, 1},
        {1, 1, 1, 0, 0, 1},
        {1, 1, 1, 1, 0, 0},
        {0, 1, 1, 0, 1, 1},
        {1, 0, 1, 1, 0, 1},
        {1, 1, 0, 1, 1, 0},
    };

    // pool
    Queue<Obstacle> obstacles = new Queue<Obstacle>(); 
    [Export] int poolSize;

    Timer delayTimer;
    [Export] float delay = 2f;
    [Export] float minDelay = .8f;
    float currentDelay;

    bool gameStart = false;

    int lastRand = 0;


    public override void _Ready()
    {
        for(int i = 0; i < GetChildCount() - 1; i++) {
            spawnPoints.Add(GetChild(i) as Position2D);
        }

        for(int i = 0; i < poolSize; i++) {
            var obstacle = (Obstacle) obstacleScene.Instance();
            obstacle.Connect("ExitScreen", this, "_on_Obstacle_ExitScreen");
            Connect("StopMove", obstacle, "_on_StopMove");
            Connect("StartMove", obstacle, "_on_StartMove");
            obstacle.ReturnState();
            obstacles.Enqueue(obstacle);
            AddChild(obstacle);
        }


        delayTimer = GetNode<Timer>("DelayTimer");

        ReturnState();

        GD.Print(valueArray.GetLength(0));


        // SpawnObstacle();
        // delayTimer.Start();
    }

    public override void _Process(float delta)
    {
        if(gameStart) {
            if(currentDelay > minDelay) {
                currentDelay -= delta / 100;
            }
        }
    }

    void _on_DelayTimer_timeout() {
        SpawnObstacle();
        delayTimer.WaitTime = currentDelay;
    }

    void SpawnObstacle() {
        var rand = GD.Randi() % valueArray.GetLength(0);

        while(rand == lastRand) {
            rand = GD.Randi() % valueArray.GetLength(0);
        }

        lastRand = (int) rand;

        for(int i = 0; i < 6; i++) {
            if(valueArray[rand,i] == 1) {
                var obstacle = obstacles.Dequeue();
                obstacle.Init(spawnPoints[i].GlobalPosition);
            }
        }

        GD.Print("rand : " + rand);
    }

    void _on_StartGame() {
        gameStart = true;
        SpawnObstacle();
        delayTimer.Start();
    }

    void _on_StopSpawn() {
        EnqueueAllObstacles();
        ReturnState();
    }

    void _on_Game_ExitGame() {
        ReturnState();
        EnqueueAllObstacles();
    }

    void ReturnState() {
        gameStart = false;
        if(!delayTimer.IsStopped()) {
            delayTimer.Stop();
        }
        currentDelay = delay;
        delayTimer.WaitTime = currentDelay;
    }

    void _on_Obstacle_ExitScreen(Obstacle obstacle) {
        obstacles.Enqueue(obstacle);
    }

    void EnqueueAllObstacles() {
        for(int i = 0; i < GetChildCount(); i++) {
            if(GetChild(i).IsInGroup("obstacle") && !obstacles.Contains(GetChild(i) as Obstacle)) {
                var obstacle = GetChild(i) as Obstacle;
                obstacle.ReturnState();
                obstacles.Enqueue(GetChild(i) as Obstacle);
            }
        }
    }

    void _on_GamePause() {
        delayTimer.Stop();
        EmitSignal("StopMove");
    }

    void _on_GameResume() {
        delayTimer.Start();
        EmitSignal("StartMove");
    }


}
