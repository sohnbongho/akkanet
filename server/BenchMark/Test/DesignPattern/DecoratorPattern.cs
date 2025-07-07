namespace BenchMark.Test.DesignPattern;

public class DecoratorPattern
{
    public interface IGameCharacter
    {
        void Attack();
    }
    public class Player : IGameCharacter
    {
        public void Attack()
        {
            Console.WriteLine("기본 공격!");
        }
    }
    public abstract class BuffDecorator : IGameCharacter
    {
        protected IGameCharacter _character;

        public BuffDecorator(IGameCharacter character)
        {
            _character = character;
        }

        public virtual void Attack()
        {
            _character.Attack();  // 기존 기능 유지
        }
    }
    public class SpeedBuff : BuffDecorator
    {
        public SpeedBuff(IGameCharacter character) : base(character) { }

        public override void Attack()
        {
            base.Attack();
            Console.WriteLine("🌀 속도가 증가했습니다! (공격 속도 +20%)");
        }
    }
    public class PowerBuff : BuffDecorator
    {
        public PowerBuff(IGameCharacter character) : base(character) { }

        public override void Attack()
        {
            base.Attack();
            Console.WriteLine("🔥 공격력이 증가했습니다! (데미지 +30%)");
        }
    }
    
    public class DefenceBuff : BuffDecorator
    {
        public DefenceBuff(IGameCharacter character) : base(character) { }

        public override void Attack()
        {
            base.Attack();
            Console.WriteLine("🔥 디펜스가 증가했습니다! (방어 +30%)");
        }
    }

    public void Test()
    {
        // 기본 플레이어 객체 생성
        IGameCharacter player = new Player();
        Console.WriteLine("#1 기본 상태:");
        player.Attack();

        Console.WriteLine("\n#2 속도 버프 적용:");
        player = new SpeedBuff(player); // SpeedBuff 데코레이터 적용
        player.Attack();

        Console.WriteLine("\n#3 공격력 버프 추가 적용:");
        player = new PowerBuff(player); // PowerBuff 데코레이터 추가 적용
        player.Attack();
        
        Console.WriteLine("\n#4 디펜스 버프 추가 적용:");
        player = new DefenceBuff(player); // PowerBuff 데코레이터 추가 적용
        player.Attack();

    }

}
