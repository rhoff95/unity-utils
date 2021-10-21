using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Test
{
    [RequireComponent(typeof(PlayerInputManager))]
    public class PlayerManager : MonoBehaviour
    {
        private PlayerInputManager _pim;

        [Range(0, 4)] public int playersToJoin = 0;
        // PlayerInputManager does not expose playerPrefab in the editor when manually joining.
        public GameObject playerPrefab;

        private void Awake()
        {
            _pim = GetComponent<PlayerInputManager>();
        }

        private void Start()
        {
            _pim.EnableJoining();

            GameObject prefab;
            switch (_pim.joinBehavior)
            {
                case PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed:
                case PlayerJoinBehavior.JoinPlayersWhenJoinActionIsTriggered:
                    Debug.Log($"Using player prefab from {nameof(PlayerInputManager)}");
                    prefab = _pim.playerPrefab;
                    break;
                case PlayerJoinBehavior.JoinPlayersManually:
                    Debug.Log($"Using player prefab from {nameof(PlayerManager)}");
                    prefab = playerPrefab;
                    _pim.playerPrefab = playerPrefab;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var playerPrefabInput = prefab.GetComponent<PlayerInput>();
            var inputActionAsset = playerPrefabInput.actions;
            var controlSchemes = inputActionAsset.controlSchemes.ToArray();
            var devices = InputSystem.devices;

            for (var i = 0; i < playersToJoin; i++)
            {
                if (i >= controlSchemes.Length)
                {
                    Debug.LogWarning($"Cannot create player ({i}), available control scheme not found.");
                    break;
                }
                var controlSchemeTarget = controlSchemes[i];

                InputControlScheme.MatchResult preferredDeviceMatch = default;
                try
                {
                    preferredDeviceMatch = controlSchemeTarget.PickDevicesFrom(devices);
                    if (preferredDeviceMatch.isSuccessfulMatch)
                    {
                        var device = preferredDeviceMatch.devices[0];
                        Debug.Log(
                            $"Joining ({prefab.name}) ({i}) with control scheme ({controlSchemeTarget.name}) and device ({device.name})");
                        _pim.JoinPlayer(i, -1, controlSchemeTarget.name, device);
                    }
                    else
                    {
                        Debug.LogError(
                            $"hasMissingOptionalDevices={preferredDeviceMatch.hasMissingOptionalDevices}, hasMissingRequiredDevices={preferredDeviceMatch.hasMissingRequiredDevices}");
                    }
                }
                finally
                {
                    preferredDeviceMatch.Dispose();
                }
            }

            _pim.DisableJoining();
        }
    }
}
