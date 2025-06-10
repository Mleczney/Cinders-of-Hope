using Godot;
using System;
using System.Collections.Generic; // ← pro Dictionary

public partial class Footsteps : Node3D
{
	private CharacterBody3D _player;
	private RayCast3D _physicsRayCheck;
	private AudioStreamPlayer3D _dynamicFootsteps;

	[Export] public float StepInterval = 0.15f;
	[Export] public float SpeedThreshold = 0.75f;

	// POZOR: Godot neumí exportovat Dictionary do editoru, ale v kódu to funguje!
	public Dictionary<string, AudioStream> SoundMapping = new()
	{
		{ "SnehKroky", GD.Load<AudioStream>("res://Sounds/SbirkaZvuku/SnehKroky.tres") },
		{ "Default", GD.Load<AudioStream>("res://Sounds/SbirkaZvuku/SnehKroky.tres") } // přidej další pokud chceš
	};

	[Export] public float PitchMin = 0.95f;
	[Export] public float PitchMax = 1.05f;

	private float _stepTimer;

	public override void _Ready()
	{
		_player = GetParent<CharacterBody3D>();
		_physicsRayCheck = GetNode<RayCast3D>("PhysicsRayCheck");
		_dynamicFootsteps = GetNode<AudioStreamPlayer3D>("DynamicFootsteps");
		_stepTimer = StepInterval;
	}

	public override void _Process(double delta)
	{
		StepInterval = _player.Get("IsSprinting").AsBool() ? 0.3f : 0.4f;

		if (_player.IsOnFloor() && _player.Velocity.Length() > SpeedThreshold)
		{
			_stepTimer -= (float)delta;
			if (_stepTimer <= 0.0f)
			{
				PlayFootsteps();
				_stepTimer = StepInterval;
			}
		}
		else
		{
			_stepTimer = StepInterval;
		}
	}

	public void PlayFootsteps()
	{
		string materialName = "Default";

		if (_physicsRayCheck.IsColliding())
		{
			var collider = _physicsRayCheck.GetCollider() as Node;

			if (collider != null)
			{
				if (collider.IsInGroup("Grass"))
					materialName = "Grass";
				else if (collider.IsInGroup("Sneh"))
					materialName = "SnehKroky";
			}
		}

		if (SoundMapping.ContainsKey(materialName))
			_dynamicFootsteps.Stream = SoundMapping[materialName];
		else
			_dynamicFootsteps.Stream = SoundMapping["Default"];

		_dynamicFootsteps.PitchScale = (float)GD.RandRange(PitchMin, PitchMax);
		_dynamicFootsteps.Play();
	}
}
