namespace MultiRobotSimulator.Abstractions
{
    public interface IAlgo
    {
        IGraph Graph { get; }
        string Name { get; }

        void Initialize(IGraph graph);

        void RunSearch();
    }
}
