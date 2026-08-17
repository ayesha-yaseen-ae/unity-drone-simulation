using UnityEngine;

public class DronePhysicsController : MonoBehaviour
{
    private ArticulationBody droneBody;

    [Header("Flight Controller Core")]
    public float maxThrustForce = 40f;    // Upward lifting capability
    public float tiltControlPower = 5f;   // How fast buttons tilt the drone
    
    [Header("Axis Stabilization (Tuning)")]
    public float pitchLevelStrength = 4.5f; // For W/S (Keep this where it felt good!)
    public float rollLevelStrength = 2.0f;  // For A/D (Lower value to stop the A/D wobble!)

    [Header("Aerodynamic Drag")]
    public float linearDamping = 4f;      // Air resistance for forward drift
    public float angularDamping = 12f;     // Rotational shock absorber

    void Start()
    {
        droneBody = GetComponent<ArticulationBody>();
        if (droneBody != null)
        {
            droneBody.linearDamping = linearDamping;
            droneBody.angularDamping = angularDamping;
            droneBody.immovable = false;
        }
    }

    void FixedUpdate()
    {
        if (droneBody == null) return;

        // 1. LIFT PHYSICS
        float currentThrust = 0f;
        if (Input.GetKey(KeyCode.Space)) currentThrust = maxThrustForce;
        else if (Input.GetKey(KeyCode.LeftShift)) currentThrust = 0f;
        else currentThrust = Mathf.Abs(Physics.gravity.y) * droneBody.mass;

        droneBody.AddForce(transform.up * currentThrust, ForceMode.Force);

        // 2. ELECTRONIC GYROSCOPE SIMULATION
        float currentPitch = Mathf.DeltaAngle(0, transform.localEulerAngles.x);
        float currentRoll = Mathf.DeltaAngle(0, transform.localEulerAngles.z);

        float targetPitch = 0f;
        float targetRoll = 0f;

        if (Input.GetKey(KeyCode.W)) targetPitch = tiltControlPower * 3f;
        if (Input.GetKey(KeyCode.S)) targetPitch = -tiltControlPower * 3f;
        if (Input.GetKey(KeyCode.A)) targetRoll = tiltControlPower * 3f;
        if (Input.GetKey(KeyCode.D)) targetRoll = -tiltControlPower * 3f;

        // Error calculation
        float pitchError = targetPitch - currentPitch;
        float rollError = targetRoll - currentRoll;

        // Apply separate axis strengths to compensate for asymmetrical moments of inertia
        float correctivePitchTorque = pitchError * pitchLevelStrength;
        float correctiveRollTorque = rollError * rollLevelStrength;

        Vector3 stabilizingTorque = (transform.right * correctivePitchTorque) + (transform.forward * correctiveRollTorque);
        droneBody.AddTorque(stabilizingTorque, ForceMode.Force);
    }
}
