namespace MultiRobotSimulator.Abstractions
{
    public interface IAlgo
    {
        IGraph Graph { get; }
        string Name { get; }

        void InitializeInternal(IGraph graph);

        void RunSearch();
    }
}
