using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Core.Utilities;

namespace Games.BattyBat
{
    public class Spawner : MonoBehaviour
    {
        [SerializeField] private Rosin _rosinPrefab;
        [SerializeField] private Transform _spawnPoint;

        [MinMaxSlider(-5f, 5f)]
        [SerializeField] private Vector2 _spawnHeightRange = new Vector2(-2f, 2f);

        public static float SpawnerTime = 1.5f;

        private ObjectPool<Rosin> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<Rosin>(
                createFunc: CreateRosin,
                actionOnGet: OnTakeRosin,
                actionOnRelease: OnReturnRosin,
                actionOnDestroy: OnDestroyRosin,
                collectionCheck: true,
                defaultCapacity: 10,
                maxSize: 40
            );
        }

        private void Start()
        {
            SpawnLoop(this.GetCancellationTokenOnDestroy()).Forget();
        }

        private Rosin CreateRosin()
        {
            Rosin rosin = Instantiate(_rosinPrefab, transform);
            rosin.SetPool(_pool);
            return rosin;
        }

        private void OnTakeRosin(Rosin rosin)
        {
            float startX = _spawnPoint ? _spawnPoint.position.x : 10f;
            float randomY = Random.Range(_spawnHeightRange.x, _spawnHeightRange.y);
            rosin.transform.position = new Vector3(startX, randomY, 0);
            rosin.gameObject.SetActive(true);
        }

        private void OnReturnRosin(Rosin rosin)
        {
            rosin.gameObject.SetActive(false);
        }

        private void OnDestroyRosin(Rosin rosin)
        {
            Destroy(rosin.gameObject);
        }

        private async UniTask SpawnLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
                    continue;
                }

                SpawnRosin();
                float delay = SpawnerTime > 0 ? SpawnerTime : 1f;
                await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: token);
            }
        }

        private void SpawnRosin()
        {
            _pool.Get();
        }
    }
}