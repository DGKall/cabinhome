using UnityEngine;

namespace CabinHome
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class WalkMotor : MonoBehaviour
    {
        private CharacterController body;
        private float fallSpeed;
        public Vector3 Eye => transform.position + Vector3.up * 1.65f;
        void Awake() => body = GetComponent<CharacterController>();

        public void Teleport(Vector3 feet)
        {
            if (!body) body = GetComponent<CharacterController>();
            body.enabled = false;
            transform.position = feet;
            fallSpeed = 0;
            body.enabled = true;
        }

        // Device-independent movement; desktop input is supplied by DemoController.
        public void Step(Vector2 move, float yaw, bool fast, float dt)
        {
            dt = Mathf.Min(dt, .05f);
            if (body.isGrounded && fallSpeed < 0) fallSpeed = -2;
            fallSpeed += Physics.gravity.y * dt;
            Vector3 local = new Vector3(move.x, 0, move.y);
            local = Vector3.ClampMagnitude(local, 1f);
            Vector3 velocity = Quaternion.Euler(0, yaw, 0) * local * (fast ? 6f : 3.2f);
            velocity.y = fallSpeed;
            body.Move(velocity * dt);
            Vector3 p = transform.position;
            // Keep the background non-playable; steep north slopes use the controller's slope limit.
            if (p.x < 1 || p.x > 159 || p.z < 1 || p.z > 119 || p.y < -5)
                Teleport(SitePlan.Ground(Mathf.Clamp(p.x,1,159), Mathf.Clamp(p.z,1,119), .1f));
        }
    }
}
