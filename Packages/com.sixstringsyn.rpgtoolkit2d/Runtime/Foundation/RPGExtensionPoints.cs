namespace SixStringSyn.RPGToolkit2D.Runtime.Foundation
{
    public interface IRPGCondition { bool IsMet(IRPGExtensionContext context); }
    public interface IRPGReward { void Grant(IRPGExtensionContext context); }
    public interface IRPGEventCommand { void Execute(IRPGExtensionContext context); }
    public interface IRPGVariableProvider { bool TryGetValue(string key, out object value); }
    public interface IRPGExtensionContext { IRPGVariableProvider Variables { get; } }
}
