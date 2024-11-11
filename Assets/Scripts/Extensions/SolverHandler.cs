using UnityEngine;

namespace Dock
{
    public class SolverHandler : MonoBehaviour
    {
        public Vector3 GoalPosition { get; set; }
        public Quaternion GoalRotation { get; set; }
        public Vector3 GoalScale { get; set; }
        public float DeltaTime { get; set; }
        public void RegisterSolver(Solver solver)
        {
            
        }
        public void UnregisterSolver(Solver solver)
        {
            
        }
    }
}
