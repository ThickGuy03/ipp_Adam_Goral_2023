using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{

    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;

    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(new MyCustomData{
        _int = 0,
        _bool=true,
    },NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission.Owner);

    public struct MyCustomData:INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message); 
        }
    }

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData prevValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + ";" + newValue._int + ";" + newValue._bool + ";" + newValue.message);
        };
    }

    private void Update()
    {
        
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
            /*
            TestServerRpc("It does something with server");
             */
            randomNumber.Value = new MyCustomData
            {
                _int=10,
                _bool=false,
                message="All your data has been hacked",
            };
           
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            spawnedObjectTransform.GetComponent<NetworkObject>().Despawn(true);
            //Destroy(spawnedObjectTransform.gameObject);
        }

        Vector3 moveDir = new Vector3(0f, 0f, 0f);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void TestServerRpc(string message)
    {
        Debug.Log("TestServerRpc " + OwnerClientId +";"+ message);
    }

}
