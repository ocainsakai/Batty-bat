//#if FUSION2
//using BulletHellTemplate.Core.Events;
//using BulletHellTemplate;
//using Fusion;
//using UnityEngine;

//public class NetworkedEventBus : NetworkBehaviour
//{
//    // Singleton para fácil acesso
//    public static NetworkedEventBus Instance { get; private set; }

//    public override void Spawned()
//    {
//        if (Instance == null) Instance = this;

//        // Registrar handlers para eventos que precisam ser sincronizados
//        EventBus.Subscribe<AnimationOnRunEvent>(OnRunEvent);
//        EventBus.Subscribe<AnimationOnIdleEvent>(OnIdleEvent);
//        EventBus.Subscribe<AnimationOnActionEvent>(OnActionEvent);
//        EventBus.Subscribe<AnimationOnReceiveDamageEvent>(OnReceiveDamageEvent);
//        EventBus.Subscribe<AnimationOnDiedEvent>(OnDiedEvent);
//    }

//    private void OnDestroy()
//    {
//        if (Instance == this) Instance = null;

//        EventBus.Unsubscribe<AnimationOnRunEvent>(OnRunEvent);
//        EventBus.Unsubscribe<AnimationOnIdleEvent>(OnIdleEvent);
//        EventBus.Unsubscribe<AnimationOnActionEvent>(OnActionEvent);
//        EventBus.Unsubscribe<AnimationOnReceiveDamageEvent>(OnReceiveDamageEvent);
//        EventBus.Unsubscribe<AnimationOnDiedEvent>(OnDiedEvent);
//    }

//    #region Event Handlers
//    private void OnRunEvent(AnimationOnRunEvent evt)
//    {
//        if (evt.Target.transform.root == transform.root && HasStateAuthority)
//        {
//            // Obter o NetworkObject do CharacterModel
//            NetworkObject targetNetObj = Runner.GetPlayerObject(evt.Target.transform.root.GetComponent<NetworkObject>().Id);
//            if (targetNetObj != null)
//            {
//                Rpc_BroadcastRunEvent(targetNetObj, evt.dir);
//            }
//        }
//    }

//    private void OnIdleEvent(AnimationOnIdleEvent evt)
//    {
//        if (evt.Target.transform.root == transform.root && HasStateAuthority)
//        {
//            Rpc_BroadcastIdleEvent(evt.Target.transform);
//        }
//    }

//    private void OnActionEvent(AnimationOnActionEvent evt)
//    {
//        if (evt.Target.transform.root == transform.root && HasStateAuthority)
//        {
//            Rpc_BroadcastActionEvent(evt.Target.transform, evt.isAttack, evt.skillIndex);
//        }
//    }

//    private void OnReceiveDamageEvent(AnimationOnReceiveDamageEvent evt)
//    {
//        if (evt.Target.transform.root == transform.root && HasStateAuthority)
//        {
//            Rpc_BroadcastDamageEvent(evt.Target.transform);
//        }
//    }

//    private void OnDiedEvent(AnimationOnDiedEvent evt)
//    {
//        if (evt.Target.transform.root == transform.root && HasStateAuthority)
//        {
//            Rpc_BroadcastDeathEvent(evt.Target.transform);
//        }
//    }
//    #endregion

//    #region RPCs
//    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
//    private void Rpc_BroadcastRunEvent(NetworkTransform targetTransform, Vector2 dir)
//    {
//        // Encontre o CharacterModel pelo transform
//        var model = targetTransform.GetComponent<CharacterModel>();
//        if (model != null)
//        {
//            EventBus.Publish(new AnimationOnRunEvent(model, dir));
//        }
//    }

//    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
//    private void Rpc_BroadcastIdleEvent(NetworkTransform targetTransform)
//    {
//        var model = targetTransform.GetComponent<CharacterModel>();
//        if (model != null)
//        {
//            EventBus.Publish(new AnimationOnIdleEvent(model));
//        }
//    }

//    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
//    private void Rpc_BroadcastActionEvent(NetworkTransform targetTransform, bool isAttack, int skillIndex)
//    {
//        var model = targetTransform.GetComponent<CharacterModel>();
//        if (model != null)
//        {
//            EventBus.Publish(new AnimationOnActionEvent(model, isAttack, skillIndex));
//        }
//    }

//    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
//    private void Rpc_BroadcastDamageEvent(NetworkTransform targetTransform)
//    {
//        var model = targetTransform.GetComponent<CharacterModel>();
//        if (model != null)
//        {
//            EventBus.Publish(new AnimationOnReceiveDamageEvent(model));
//        }
//    }

//    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
//    private void Rpc_BroadcastDeathEvent(NetworkTransform targetTransform)
//    {
//        var model = targetTransform.GetComponent<CharacterModel>();
//        if (model != null)
//        {
//            EventBus.Publish(new AnimationOnDiedEvent(model));
//        }
//    }
//    #endregion
//}
//#endif