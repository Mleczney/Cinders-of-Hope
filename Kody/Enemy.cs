using Godot;

namespace cslearn;

public partial class Enemy : CharacterBody3D
{
    [Export] public int Health = 100;
    private ProgressBar _healthBar;
    private NavigationAgent3D _navAgent;
    private CharacterBody3D _player;
    [Export] public bool _chasing = false;
    public Node3D _bodyMesh;
    public GpuParticles3D _particles;
    private CharacterBody3D _enemy;

    float speed = 3.5f;
    private bool _alreadyHit = false;

    public override void _Ready()
    {
        _healthBar = GetNode<ProgressBar>("SubViewport/ProgressBar");
        _navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        _bodyMesh = GetNode<Node3D>("BodyMesh");
        _particles = GetNode<GpuParticles3D>("BodyMesh/Mesh/GPUParticles3D");
        _enemy = GetNode<CharacterBody3D>(".");

        // Najdeme hráče ve stromu scény
        _player = GetTree().CurrentScene.GetNode<CharacterBody3D>("Player");
    }

    public override void _PhysicsProcess(double delta)
    {
        _navAgent.TargetPosition = _player.GlobalPosition;
        Vector3 direction = (_navAgent.GetNextPathPosition() - GlobalPosition).Normalized();
        Velocity = direction * speed;
    
        // Otáčení těla směrem k hráči
        Vector3 lookDir = _player.GlobalPosition - _bodyMesh.GlobalPosition;
        lookDir.Y = 0; // Zamezíme naklánění nahoru/dolů
        if (lookDir.Length() > 0.01f)
        {
            _bodyMesh.LookAt(_bodyMesh.GlobalPosition + lookDir, Vector3.Up);
        }

        MoveAndSlide();
    }

    
    public override void _Process(double delta)
    {
        if (Health <= 0)
        {
            _particles.Emitting = true;
            ParticlesFinished();
        }
    }

    public void ParticlesFinished()
    {
        _enemy.QueueFree();
    }

    private void _on_Hitbox_area_entered(Area3D area)
    {
        if (area.Name == "DamageArea" && !_alreadyHit)
        {
            TakeDamage(10);
            _alreadyHit = true;
        }
    }

    public void TakeDamage(int damage)
    {
        Health = Mathf.Max(Health - damage, 0);
        _healthBar.Value = Health;
    }

    public void ResetHit()
    {
        _alreadyHit = false;
    }
}