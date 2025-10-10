using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spaceship : MonoBehaviour
{
	private Rigidbody2D rb;
	private float moveSpeed = 20f;
	private float dirX = 0f;

	public Text text; // UI Text to show gyroscope data

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();

		// Enable gyro if available
		if (!SystemInfo.supportsGyroscope)
		{
			Debug.LogWarning("Gyroscope not supported on this device");
		}
		else
		{
			Input.gyro.enabled = true;
		}
	}

	void Update()
	{

		// Get rotation rate or user acceleration
		Vector3 tilt = Input.gyro.rotationRateUnbiased;

		// Show value on UI text
		if (text != null)
			text.text = $"X: {tilt.x:F2}, Y: {tilt.y:F2}, Z: {tilt.z:F2}";

		// Use X tilt for movement (you can switch to .y for up/down)
		dirX = tilt.x * moveSpeed;

		// Limit position (so the ship doesn't go off screen)
		transform.position = new Vector2(
			Mathf.Clamp(transform.position.x, -7.5f, 7.5f),
			transform.position.y
		);
	}

	void FixedUpdate()
	{
		rb.linearVelocity = new Vector2(dirX, 0f);
	}
}
