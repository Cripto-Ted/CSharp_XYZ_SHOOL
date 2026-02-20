using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        private List<Vector2Int> _targetsToMove = new List<Vector2Int>();

        public override string TargetUnitName => "Cobra Commando";

        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;

        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        protected override void GenerateProjectiles(Vector2Int forTarget, List<BaseProjectile> intoList)
        {
            if (GetTemperature() >= OverheatTemperature)
                return;

            IncreaseTemperature();

            int shotsCount = Mathf.Max(1, (int)_temperature);

            for (int i = 0; i < shotsCount; i++)
            {
                var projectile = CreateProjectile(forTarget);
                AddProjectileToList(projectile, intoList);
            }
        }

        public override Vector2Int GetNextStep()
        {
            if (_targetsToMove.Count == 0)
                return Position;

            var target = _targetsToMove[0];

            if (IsTargetReachable(target))
                return Position;

            return Position.CalcNextStepTowards(target);
        }

        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>();
            var allTargets = GetAllTargets();

            _targetsToMove.Clear();

            if (allTargets.Count > 0)
            {
                Vector2Int mostDangerous = allTargets[0];
                float closestDistance = DistanceToOwnBase(mostDangerous);

                foreach (var target in allTargets)
                {
                    float distance = DistanceToOwnBase(target);

                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        mostDangerous = target;
                    }
                }

                _targetsToMove.Add(mostDangerous);

                if (IsTargetReachable(mostDangerous))
                    result.Add(mostDangerous);
            }
            else
            {
                var enemyBase = runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerID
                ];

                var basePosition = enemyBase.Position;

                _targetsToMove.Add(basePosition);

                if (IsTargetReachable(basePosition))
                    result.Add(basePosition);
            }

            return result;
        }

        public override void Update(float deltaTime, float time)
        {
            if (_overheated)
            {
                _cooldownTime += deltaTime;

                float t = _cooldownTime / OverheatCooldown;

                _temperature = Mathf.Lerp(OverheatTemperature, 0f, t);

                if (t >= 1f)
                {
                    _cooldownTime = 0f;
                    _overheated = false;
                }
            }
        }

        private int GetTemperature()
        {
            if (_overheated)
                return (int)OverheatTemperature;

            return (int)_temperature;
        }

        private void IncreaseTemperature()
        {
            _temperature += 1f;

            if (_temperature >= OverheatTemperature)
                _overheated = true;
        }
    }
}
