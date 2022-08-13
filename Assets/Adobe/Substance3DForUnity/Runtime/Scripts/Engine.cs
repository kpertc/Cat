using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace Adobe.Substance
{
    public class Engine
    {
        private enum LoadState : uint
        {
            Engine_Unloaded = 0x00u, //!< Engine is currently not loaded
            Engine_Loaded = 0x02u, //!< The engine is loaded
            Engine_FatalError = 0x04u, //!< An unrecoverable error has occurred
        }

        private static LoadState sLoadState = LoadState.Engine_Unloaded;

        public static bool IsInitialized => sLoadState == LoadState.Engine_Loaded;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        private const int MAX_TEXTURE_SIZE = 512;
        private const int MEMORY_BUGET = 512;
#else
        private const int MEMORY_BUGET = 2048;
#endif


        public static string Version()
        {
            var version_ptr = NativeMethods.sbsario_get_version();
            return Marshal.PtrToStringAnsi(version_ptr);
        }

        public static string GetHash()
        {
            var version_ptr = NativeMethods.sbsario_get_hash();
            return Marshal.PtrToStringAnsi(version_ptr);
        }

        //! @brief Initialize the Substance Engine
        //! @param modulePath Path to the native module, only used if native module
        //!                   dynamic loading is enabled in the managed library
        //! @param enginePath Path to the Substance engine on disk, only used if
        //!                   dynamic engine loading is enabled in the native library
        public static void Initialize(string pluginPath, string enginePath)
        {
            if (sLoadState == LoadState.Engine_Loaded)
                return;

            IntPtr memoryBudget = (IntPtr)MEMORY_BUGET;

            var code = (ErrorCode)NativeMethods.sbsario_initialize(pluginPath, enginePath, memoryBudget);

            // On success, set the engine state to loaded
            if (code == ErrorCode.SBSARIO_ERROR_OK)
                sLoadState = LoadState.Engine_Loaded;

            if (code != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(code);
        }

        public static void Initialize()
        {
            if (sLoadState == LoadState.Engine_Loaded)
                return;

            IntPtr memoryBudget = (IntPtr)MEMORY_BUGET;

            var code = (ErrorCode)NativeMethods.sbsario_initialize(null, null, memoryBudget);

            if (sLoadState == LoadState.Engine_Unloaded)
            {
                // On success, set the engine state to loaded
                if (code == ErrorCode.SBSARIO_ERROR_OK)
                    sLoadState = LoadState.Engine_Loaded;
            }

            if (code != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(code);
        }

        public static void Shutdown()
        {
            var code = (ErrorCode)NativeMethods.sbsario_shutdown();

            if (sLoadState == LoadState.Engine_Loaded)
            {
                // On success, set the engine to an unloaded state
                if (code == ErrorCode.SBSARIO_ERROR_OK)
                    sLoadState = LoadState.Engine_Unloaded;
            }

            if (code != ErrorCode.SBSARIO_ERROR_OK)
                throw new SubstanceException(code);
        }

        public static SubstanceNativeHandler OpenFile(string path)
        {
            var substanceFile = new SubstanceNativeHandler(path);
            return substanceFile;
        }

        public static SubstanceNativeHandler OpenFile(byte[] data)
        {
            var substanceFile = new SubstanceNativeHandler(data);
            return substanceFile;
        }

        private static double InvPow(double pBase, double pResult)
        {
            double exposent = Math.Log(pResult) / Math.Log(pBase);
            return exposent;
        }

        private static bool IsPowerOfTwo(double number)
        {
            double log = Math.Log(number, 2);
            double pow = Math.Pow(2, Math.Round(log));
            return pow == number;
        }
    }
}