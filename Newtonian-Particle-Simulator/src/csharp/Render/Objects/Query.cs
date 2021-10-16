using System;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;

namespace Newtonian_Particle_Simulator.Render.Objects
{
    /// <summary>
    /// This measures the timings of GPU commands
    /// </summary>
    class Query : IDisposable
    {
        public readonly int ID;
        public float ElapsedMilliseconds { get; private set; }

        private readonly Stopwatch timer = new Stopwatch();
        private bool doStopAndReset = false;


        public int UpdateRate;
        public Query(int updatePeriodInMs)
        {
            GL.CreateQueries(QueryTarget.TimeElapsed, 1, out ID);
            UpdateRate = updatePeriodInMs;
        }


        /// <summary>
        /// If <seealso cref="UpdateRate"/> milliseconds are elapsed since the last Query, a new Query on the GPU, which captures all render commands from now until StopAndReset, is started.
        /// </summary>
        public void Start()
        {
            if (!timer.IsRunning || timer.ElapsedMilliseconds >= UpdateRate)
            {
                GL.BeginQuery(QueryTarget.TimeElapsed, ID);
                doStopAndReset = true;
                timer.Restart();
            }
        }

        /// <summary>
        /// Resets the Query on the GPU and stores the result in <seealso cref="ElapsedMilliseconds"/>
        /// </summary>
        public void StopAndReset()
        {
            if (doStopAndReset)
            {
                GL.EndQuery(QueryTarget.TimeElapsed);
                GL.GetQueryObject(ID, GetQueryObjectParam.QueryResult, out int elapsedTime);
                ElapsedMilliseconds = elapsedTime / 1000000f;
                doStopAndReset = false;
            }
        }

        public void Dispose()
        {
            GL.DeleteQuery(ID);
        }
    }
}
