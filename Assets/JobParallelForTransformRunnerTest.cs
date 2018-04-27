using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class JobParallelForTransformRunnerTest : MonoBehaviour {
    struct RotationJob : IJobParallelFor {
        [ReadOnly] public NativeArray<Vector3> velocity;
        public NativeArray<Vector3> rotation;
        public float deltaTime;

        public void Execute(int index) {
//            var sum = velocity.Sum(x => x.magnitude); //junk work
            rotation[index] = rotation[index] + velocity[index] * deltaTime;
        }
    }
    
    struct ApplyTransformJob : IJobParallelForTransform {
        [ReadOnly] public NativeArray<Vector3> rotation;

        public void Execute(int index, TransformAccess transform) {
            transform.rotation = Quaternion.Euler(rotation[index]);
        }
    }

    [SerializeField] private PrimitiveType type = PrimitiveType.Cube;
    [SerializeField] private int count = 500;
    [SerializeField] private int speedMultiplier = 5;
    [SerializeField] private int innerloopBatchCount = 100;

    private GameObject particle;
    private GameObject[] cubes;
    private NativeArray<Vector3> rotation;
    private NativeArray<Vector3> velocity;
    private TransformAccessArray jobCubes;
    private JobHandle rotJobHandle;
    private JobHandle applyJobHandle;

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
        jobCubes = new TransformAccessArray(cubes.Select(x => x.transform).ToArray(), cubes.Length);
    }

    // Update is called once per frame
    void Update() {
        print(rotJobHandle.IsCompleted);

        if (rotJobHandle.IsCompleted && applyJobHandle.IsCompleted) {
            rotJobHandle.Complete();
            var job = new RotationJob() {
                deltaTime = Time.deltaTime,
                rotation = rotation,
                velocity = velocity
            };
            
            rotJobHandle = job.Schedule(rotation.Length, innerloopBatchCount, applyJobHandle);
        }

        if (applyJobHandle.IsCompleted) {
            
            applyJobHandle.Complete();
            var applyJob = new ApplyTransformJob() {
                rotation = rotation
            };
            
            applyJobHandle = applyJob.Schedule(jobCubes, rotJobHandle);           
        }
    }

    private void OnApplicationQuit() {
        rotJobHandle.Complete();
        rotation.Dispose();
        velocity.Dispose();
        jobCubes.Dispose();
    }
}