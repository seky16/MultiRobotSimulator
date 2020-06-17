using MultiRobotSimulator.Abstractions;
using static System.Math;

namespace MultiRobotSimulator.Helpers
{
    public static class Metrics
    {
        /// <summary>
        /// <para>d=sqrt((x1-x2)^2+(y1-y2)^2)</para>
        /// <para>See https://en.wikipedia.org/wiki/Euclidean_metric </para>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double Euclidean(AbstractTile source, AbstractTile target)
            => Sqrt(Pow(source.X - target.X, 2) + Pow(source.Y - target.Y, 2));

        /// <summary>
        /// <para>d=max(abs(x1-x2);abs(y1-y2))</para>
        /// <para>See https://en.wikipedia.org/wiki/Chebyshev_metric </para>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double Chebyshev(AbstractTile source, AbstractTile target)
            => Max(Abs(source.X - target.X), Abs(source.Y - target.Y));

        /// <summary>
        /// <para>d=abs(x1-x2)+abs(y1-y2)</para>
        /// <para>See https://en.wikipedia.org/wiki/Manhattan_metric </para>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double Manhattan(AbstractTile source, AbstractTile target)
            => Abs(source.X - target.X) + Abs(source.Y - target.Y);

        /// <summary>
        /// <para>d=max(abs(x1-x2);abs(y1-y2))+(sqrt(2)-1)*min(abs(x1-x2);abs(y1-y2))</para>
        /// <para>See https://theory.stanford.edu/~amitp/GameProgramming/Heuristics.html#diagonal-distance </para>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static double Octile(AbstractTile source, AbstractTile target)
            => Max(Abs(source.X - target.X), Abs(source.Y - target.Y)) + ((Sqrt(2) - 1) * Min(Abs(source.X - target.X), Abs(source.Y - target.Y)));
    }
}
