using BulletHellTemplate;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace BulletHellTemplate
{
    /// <summary>
    /// LifetimeScope that binds the chosen backend implementation to IBackendService.
    /// Place it in the first scene that is loaded.
    /// </summary>
    public sealed class BackendLifetimeScope : LifetimeScope
    {
        [SerializeField] private BackendSettings settings;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(settings);

            switch (settings.option)
            {
                case BackendOption.Offline:
                    builder.Register<OfflineBackendService>(Lifetime.Singleton)
                           .As<IBackendService>();
                    break;

                case BackendOption.Firebase:
                    builder.Register<FirebaseBackendService>(Lifetime.Singleton)
                           .As<IBackendService>();
                    break;

                case BackendOption.WebSocketSql:
                    builder.Register<WebSocketSqlBackendService>(Lifetime.Singleton)
                           .As<IBackendService>();
                    break;
            }

            builder.RegisterComponentInHierarchy<BackendManager>();

        }

    }
}