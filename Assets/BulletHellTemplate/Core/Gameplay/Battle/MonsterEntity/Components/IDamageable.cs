namespace BulletHellTemplate
{
    /// <summary>Any object that can receive damage points.</summary>
    public interface IDamageable
    {
        void ReceiveDamage(float amount, bool critical = false);
    }
}
