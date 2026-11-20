using UnityEngine;
using Vchod.Core;
using Vchod.Player;
using Vchod.World;

namespace Vchod.Systems
{
    /// <summary>Feeds the player's position into the CO model and announces stage changes and collapse.</summary>
    public sealed class CoSystem : MonoBehaviour
    {
        [SerializeField] private CoConfig config;
        [SerializeField] private PlayerController player;
        [SerializeField] private ModuleLayoutBuilder house;

        public CoModel Model { get; private set; }
        public CoConfig Config => config;
        public float Level => Model.Level;
        public int Stage => Model.Stage;
        public float DistanceToSource { get; private set; } = float.PositiveInfinity;

        private void Awake() => Model = new CoModel(config.model);

        private void Update()
        {
            if (house.Plan == null) return;

            var position = player.transform.position;
            DistanceToSource = Vector3.Distance(position, house.CoSourcePosition);
            var exposure = new CoExposure
            {
                DistanceToSource = DistanceToSource,
                InStairwell = house.IsInStairwell(position),
                AtOpenWindow = false, // windows open in T-023 (M1)
            };

            var previous = Model.Stage;
            if (!Model.Tick(Time.deltaTime, exposure)) return;

            EventBus.Publish(new CoStageChanged { PreviousStage = previous, Stage = Model.Stage, Level = Model.Level });
            if (Model.IsCollapsed)
            {
                player.SetInputLock(this, true);
                EventBus.Publish(new CoCollapsed { Level = Model.Level });
            }
        }
    }
}
