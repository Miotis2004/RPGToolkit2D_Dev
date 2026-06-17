using System;

namespace SixStringSyn.RPGToolkit2D.Runtime.Stats
{
    public sealed class ResourcePool
    {
        public ResourcePool(ResourceDefinition definition, float maximum, float current = -1f)
        {
            Definition = definition;
            Maximum = Math.Max(0f, maximum);
            Current = current < 0f ? Maximum : Clamp(current);
        }

        public event Action<ResourcePool, float, float> Changed;

        public ResourceDefinition Definition { get; }
        public float Current { get; private set; }
        public float Maximum { get; private set; }
        public float Normalized => Maximum <= 0f ? 0f : Current / Maximum;

        public void SetMaximum(float maximum, bool fillToMaximum = false)
        {
            var before = Current;
            Maximum = Math.Max(0f, maximum);
            Current = fillToMaximum ? Maximum : Clamp(Current);
            RaiseIfChanged(before);
        }

        public float Consume(float amount) => Change(-Math.Max(0f, amount));
        public float Restore(float amount) => Change(Math.Max(0f, amount));
        public float Regenerate(float deltaSeconds) => Restore((Definition?.RegenerationPerSecond ?? 0f) * Math.Max(0f, deltaSeconds));

        private float Change(float delta)
        {
            var before = Current;
            Current = Clamp(Current + delta);
            RaiseIfChanged(before);
            return Math.Abs(Current - before);
        }

        private float Clamp(float value) => Math.Max(0f, Math.Min(Maximum, value));

        private void RaiseIfChanged(float before)
        {
            if (!before.Equals(Current))
            {
                Changed?.Invoke(this, before, Current);
            }
        }
    }
}
