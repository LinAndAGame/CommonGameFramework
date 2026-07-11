namespace CardMaster.Framework.Buff
{
    public abstract class BuffInstance
    {
        public BaseBuff Definition { get; }
        public object Owner { get; }
        public string BuffId => Definition?.BuffId;
        public int CurLayer { get; set; } = 1;
        public IBuffDuration Duration { get; set; }

        protected BuffInstance(BaseBuff definition, object owner)
        {
            Definition = definition;
            Owner = owner;
        }

        public virtual void OnApply() { }

        public virtual void OnRemove() { }

        public void Tick(float deltaTime) => Duration?.Tick(deltaTime);
        public bool IsExpired => Duration != null && Duration.IsExpired == true;
    }
}
