namespace BugZoo.Steps.Step05
{
    public sealed class EnemyBrain
    {
        private float _attackTimer;

        public EnemyState State { get; private set; } = EnemyState.Idle;

        public void Tick(float deltaTime, bool wasHit)
        {
            // 同じフレームに重なったら、ひるみを優先する（ルールをここ1か所で決める）
            if (wasHit)
            {
                State = EnemyState.Stun;
                return;
            }

            _attackTimer += deltaTime;
            if (_attackTimer >= 2f)
            {
                _attackTimer = 0f;
                State = EnemyState.Attack;
            }
        }
    }
}
