namespace Betreten_Verboten.Components.BV.Player
{
    public abstract class BVPlayer : Base.Player
    {
        //ctor
        public BVPlayer(int Nr) : base(Nr) { }


        public virtual int AngerCount { get; set; }
        public int AdditionalPoints { get; set; }
        public override int Points => AdditionalPoints;
    }
}
