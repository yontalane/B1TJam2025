namespace B1TJam2025
{
    public interface IHittable
    {
        public bool IsDead { get; }
        public void GetHit();
        public void GetKilled();
    }
}
