using BugZoo.Part1.Q09;
using NUnit.Framework;

namespace BugZoo.Tests.EditMode
{
    public sealed class EnemyBrainTests
    {
        [Test]
        public void Tick_2秒経過するとAttackになる()
        {
            var brain = new EnemyBrain();
            brain.Tick(2.0f, wasHit: false);
            Assert.AreEqual(EnemyState.Attack, brain.State);
        }

        [Test]
        public void Tick_同じフレームに攻撃とひるみが重なったらStunが優先される()
        {
            var brain = new EnemyBrain();
            brain.Tick(2.0f, wasHit: true);
            Assert.AreEqual(EnemyState.Stun, brain.State);
        }

        [Test]
        public void Tick_1秒ではIdleのまま()
        {
            var brain = new EnemyBrain();
            brain.Tick(1.0f, wasHit: false);
            Assert.AreEqual(EnemyState.Idle, brain.State);
        }

        [Test]
        public void Tick_Attackになったらタイマーが0に戻る()
        {
            var brain = new EnemyBrain();
            brain.Tick(2.0f, wasHit: false);
            Assert.AreEqual(EnemyState.Attack, brain.State);

            brain.Tick(0.0f, wasHit: true);
            Assert.AreEqual(EnemyState.Stun, brain.State);
        }
    }
}
