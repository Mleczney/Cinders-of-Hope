using System;
using Godot;
using GpuParticles3D = Godot.GpuParticles3D;

namespace cslearn;

public partial class PlayerController : CharacterBody3D
{
    [Export] public float Speed = 5.0f;
    [Export] public float WalkSpeed = 5.0f;
    [Export] public float SprintSpeed = 7.0f;
    [Export] public float JumpVelocity = 4.5f;
    [Export] public float Sensitivity = 0.001f;

    // Head & Camera
    private Node3D _head;
    private Camera3D _camera;

    // Body and Attack
    private Node3D _bodyMesh;
    private AnimationPlayer _handAnim;
    private CollisionShape3D _attackCollision;
    private bool _isAttacking;

    public bool IsSprinting;

    // UI
    private Label _uiLabel;

    // Control
    private bool _canMove = true;
    private bool _weaponEquiped = false;


    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        InitializeNodes();
    }

    private void InitializeNodes()
    {
        _head = GetNode<Node3D>("HeadNode");
        _camera = GetNode<Camera3D>("HeadNode/HeadCamera");
        _bodyMesh = GetNode<Node3D>("BodyMesh");
        _handAnim = GetNode<AnimationPlayer>("HandAnimations");
        _attackCollision = GetNode<CollisionShape3D>("HeadNode/HeadCamera/Hand/DamageArea/Dmg");
        _uiLabel = GetTree().Root.GetNode<Label>("Node3D/AttackText");

    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Input.IsActionJustPressed("esc"))
        {
            Input.MouseMode = Input.MouseModeEnum.Confined;
            _canMove = false;
        }

        if (!_canMove) return;
        if (@event is not InputEventMouseMotion motionEvent) return;

        HandleMouseLook(motionEvent);
    }

    private void HandleMouseLook(InputEventMouseMotion motionEvent)
    {
        _head.RotateY(-motionEvent.Relative.X * Sensitivity);
        _camera.RotateX(-motionEvent.Relative.Y * Sensitivity);

        _camera.Rotation = _camera.Rotation with
        {
            
            X = Mathf.Clamp(_camera.Rotation.X, Mathf.DegToRad(-70), Mathf.DegToRad(80))
        };
    }

    public override void _Input(InputEvent @event)
    {
        if (_canMove)
        {
            HandleAttackInput(@event);
        }
    }

    private void HandleAttackInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mouseEvent) return;

        if (mouseEvent.ButtonIndex != MouseButton.Left || !mouseEvent.Pressed) return;
        if (_handAnim.IsPlaying()) return;
        _handAnim.Play("KnifeAttack");
    }
    

    public override void _Process(double delta)
    {
        UpdateUi();

        if (_canMove) UpdateAttackState();
    }

    private void UpdateUi()
    {
        _uiLabel.Text = _isAttacking.ToString();
    }

    private void UpdateAttackState()
    {
        bool isPlaying = _handAnim.IsPlaying();

        if (!isPlaying && _isAttacking) // Animace skončila a dřív hráč útočil
        {
            ResetAllEnemies(); // <- Zavoláme reset
        }

        _isAttacking = isPlaying;
    }

    public void EnableAttackCollision()
    {
        _attackCollision.Disabled = false;
    }

    public void DisableAttackCollision()
    {
        _attackCollision.Disabled = true;
    }

    private void ResetAllEnemies()
    {
        foreach (var enemy in GetTree().GetNodesInGroup("Enemies"))
        {
            if (enemy is Enemy e)
            {
                e.ResetHit();
            }
        }
    }

    private void Sprint()
    {
        Speed = Input.IsActionPressed("Sprint") ? SprintSpeed : WalkSpeed;
        if (Speed == SprintSpeed)
        {
            IsSprinting = true;
        }
        else
        {
            IsSprinting = false;
        }
    }

    private void EquipWeapon()
    {
        
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (!_canMove) return;

        var velocity = Velocity;

        if (!IsOnFloor())
            velocity += GetGravity() * (float)delta;

        if (Input.IsActionJustPressed("jump"))
            velocity.Y = JumpVelocity;

        var inputDir = Input.GetVector("left", "right", "front", "back");
        var direction = (_head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = 0;
            velocity.Z = 0;
        }


        _bodyMesh.Rotation = _head.Rotation;

        Velocity = velocity;
        Sprint();
        MoveAndSlide();
    }
}
