using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class JobRunnerTest : MonoBehaviour {
    struct VelocityJob : IJob {
        [ReadOnly] public NativeArray<Vector3> velocity;
        public NativeArray<Vector3> rotation;
        public float deltaTime;

        public void Execute() {
            for (int i = 0; i < rotation.Length; i++) {
//                var sum = velocity.Sum(x => x.magnitude); // junk work
                rotation[i] = rotation[i] + velocity[i] * deltaTime;
            }
        }
    }

    [SerializeField] private PrimitiveType type = PrimitiveType.Cube;
    [SerializeField] private int count = 500;
    [SerializeField] private int speedMultiplier = 5;

    private GameObject particle;
    private GameObject[] cubes;
    private NativeArray<Vector3> rotation;
    private NativeArray<Vector3> velocity;
    private JobHandle jobHandle;

    // Use this for initialization
    void Start() {
        particle = GameObject.CreatePrimitive(type);
        cubes = new GameObject[count];
        rotation = new NativeArray<Vector3>(count, Allocator.Persistent);
        velocity = new NativeArray<Vector3>(count, Allocator.Persistent);

        for (int i = 0; i < velocity.Length; i++) {
            velocity[i] = new Vector3(Random.value, Random.value, Random.value) * speedMultiplier;
        }

        var spawner = new Spawner(particle, rotation.Length);
        cubes = spawner.Spawn();
    }

    // Update is called once per frame
    void Update() {
        if (jobHandle.IsCompleted) {
            jobHandle.Complete();
            for (var i = 0; i < rotation.Length; i++) {
                cubes[i].transform.rotation = Quaternion.Euler(rotation[i]);
            }

            var job = new VelocityJob() {
                deltaTime = Time.deltaTime,
                rotation = rotation,
                velocity = velocity
            };

            jobHandle = job.Schedule();
        }
    }

    private void OnApplicationQuit() {
        jobHandle.Complete();
        rotation.Dispose();
        velocity.Dispose();
    }
}