using System.Collections.Generic;
using Model.Runtime.Projectiles;
using UnityEngine;

namespace UnitBrains.Player
{
    public class SecondUnitBrain : DefaultPlayerUnitBrain
    {
        // ====== A. Статический счетчик ======
        private static int _unitCounter = 0;
        private int _unitNumber;

        private const int MaxSmartTargets = 3;

        private List<Vector2Int> _targetsToMove = new List<Vector2Int>();

        public override string TargetUnitName => "Cobra Commando";

        private const float OverheatTemperature = 3f;
        private const float OverheatCooldown = 2f;

        private float _temperature = 0f;
        private float _cooldownTime = 0f;
        private bool _overheated;

        // ====== Инициализация номера юнита ======
        protected override void OnEnable()
        {
            base.OnEnable();

            _unitNumber = _unitCounter;
            _unitCounter++;
        }

        // ====== Выбор целей ======
        protected override List<Vector2Int> SelectTargets()
        {
            var result = new List<Vector2Int>();
            _targetsToMove.Clear();

            var allTargets = GetAllTargets();

            // Если врагов нет — идем на базу
            if (allTargets.Count == 0)
            {
                var enemyBase = runtimeModel.RoMap.Bases[
                    IsPlayerUnitBrain ? RuntimeModel.BotPlayerId : RuntimeModel.PlayerID
                ];

                allTargets.Add(enemyBase.Position);
            }

            // Сортируем по расстоянию до своей базы
            SortByDistanceToOwnBase(allTargets);

            // Определяем индекс цели
            int smartIndex = _unitNumber % MaxSmartTargets;

            if (smartIndex >= allTargets.Count)
                smartIndex = 0;

            var selectedTarget = allTargets[smartIndex];

            _targetsToMove.Add(selectedTarget);

            if (IsTargetReachable(selectedTarget))
                result.Add(selectedTarget);

            return result;
        }

        // ====== Движение ======
        public override Vector2Int GetNextStep()
        {
            if (_targetsToMove.Count == 0)
                return Position;

            var target = _targetsToMove[0];

            if (IsTargetReachable(target))
                return Position;

            return Position.CalcNextStepTowards(target);
        }

        // ====== Стрельба ======
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

        // ====== Обновление ======
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
