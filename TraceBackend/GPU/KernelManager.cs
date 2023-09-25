using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Cloo;
using OpenCL;
using System.Diagnostics;
using Alea;
using Alea.CSharp;
using Alea.Parallel;


namespace TraceBackend.GPU
{
    static class KernelManager
    {
        static bool useGPU;
        static Gpu _GPU;

        static KernelManager()
        {
            _GPU = Gpu.Default;
            if (_GPU == null)
                useGPU = false;
            else
                useGPU = true;
        }

        public static void For(int fromInclusive, int toExclusive, Action<int> op)
        {
            if (useGPU)
                _GPU.For(fromInclusive, toExclusive, op);
            else
                Parallel.For(fromInclusive, toExclusive, op);
        }

        /*
        static double[] SquareGPU(double[] inputs)
        {
            var worker = Worker.Default;
            using (var dInputs = worker.Malloc(inputs))
            using (var dOutputs = worker.Malloc(inputs.Length))
            {
                const int blockSize = 256;
                var numSm = worker.Device.Attributes.MULTIPROCESSOR_COUNT;
                var gridSize = Math.Min(16 * numSm,
                                        Common.divup(inputs.Length, blockSize));
                var lp = new LaunchParam(gridSize, blockSize);
                worker.Launch(SquareKernel, lp, dOutputs.Ptr, dInputs.Ptr,
                              inputs.Length);
                return dOutputs.Gather();
            }
        }

        static void SquareKernel(deviceptr outputs,
                         deviceptr inputs, int n)
        {
            var start = blockIdx.x * blockDim.x + threadIdx.x;
            var stride = gridDim.x * blockDim.x;
            for (var i = start; i < n; i += stride)
            {
                outputs[i] = inputs[i] * inputs[i];
            }
        }*/


        public static void GetPrimes()
        {
            int[] seq = Enumerable.Repeat(0, 1000).ToArray();
            int[] seq2 = Enumerable.Repeat(0, 1000).ToArray();
            int N = seq.Length;
            EasyCL cl = new EasyCL();
            //cl.ProgressChangedEvent += Cl_ProgressChangedEvent;
            cl.Accelerator = AcceleratorDevice.GPU;
            cl.LoadKernel(MakeSequence);                  //Load kernel string here, (Compiles in the background)
            cl.Invoke("MakeSequence", 100, seq.Length, seq);

            Debug.WriteLine("Should be done");
            cl.Invoke("MakeSequence", 100, seq.Length, seq2);
            //return seq ;                           //Primes now contains all Prime Numbers
            Debug.WriteLine("seq 10 = " + seq[10].ToString());
        }

        private static void Cl_ProgressChangedEvent(object sender, double e)
        {
                Console.WriteLine(e.ToString("0.00%"));
        }

        static string MakeSequence
        {
            get
            {
                return @"
                    kernel void MakeSequence(__global int* message)
                    {
                        int index = get_global_id(0);
                        int oldmessage = message[index];
                        message[index] = 1;
                        printf(""index %d: %d  ->  %d \n"",index,oldmessage, message[index] );
                    }";
            }
        }
    }
}
