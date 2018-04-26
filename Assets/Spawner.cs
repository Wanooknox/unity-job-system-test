using UnityEngine;

public class Spawner : Object {
    private readonly GameObject toSpawn;
    private readonly int count;

    public Spawner(GameObject toSpawn, int count) {
        this.toSpawn = toSpawn;
        this.count = count;
    }

    public GameObject[] Spawn() {
        var objects = new GameObject[count];
        for (int i = 0; i < count; i++) {
            GameObject obj = Instantiate(toSpawn, (Vector3.one * i), Quaternion.identity);
            objects[i] = obj;
        }

        return objects;
    }
}
