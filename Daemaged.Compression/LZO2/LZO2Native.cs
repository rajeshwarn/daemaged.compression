﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Daemaged.Compression.LZO2
{
  public enum LZOMethod { M1x15, M1x999 };

  [SuppressUnmanagedCodeSecurity]
  unsafe class LZO2Native
  {
#if MIXED_MODE
      internal const string LZO2 = "Daemaged.Compression.dll";
#else
    internal const string LZO2 = "liblzo2.dll";
#endif

    static LZO2Native()
    {
      LZOInit();
    }

    private const uint LZO_VERSION = 0x2060;

    private static int LZOInit()
    {
      return __lzo_init_v2(LZO_VERSION, sizeof(short), sizeof(int), IntPtr.Size, sizeof(uint), sizeof(uint), IntPtr.Size,
                    IntPtr.Size, IntPtr.Size, IntPtr.Size);
    }

    [DllImport(LZO2, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    private static extern int __lzo_init_v2(uint version, int szShort, int szInt, int szLong, int szLzoUint, int szUint, int szDict, int szCharP, int szVoidP, int szCallBack);


    public static readonly int LZO1X_1_15_MEM_COMPRESS = (int)(32768 * IntPtr.Size);

    [DllImport(LZO2)]
    public static extern int lzo1x_1_15_compress(byte* src, IntPtr src_len,
                                byte* dst, IntPtr* dst_len,
                                void* wrkmem);

    [DllImport(LZO2)]
    public static extern int lzo1x_decompress(byte* src, IntPtr src_len,
                                byte* dst, IntPtr* dst_len,
                                void* wrkmem /* NOT USED */ );

    [DllImport(LZO2)]
    public static extern int lzo1x_999_compress(byte* src, IntPtr src_len,
                                byte* dst, IntPtr* dst_len,
                                void* wrkmem);

    public static void Compress(LZOMethod method, byte* src, IntPtr srcLen, byte* dst, IntPtr* dstLen, void* wrkMem)
    {
      int ret;
      switch (method)
      {
        case LZOMethod.M1x15:
          ret = lzo1x_1_15_compress(src, srcLen, dst, dstLen, wrkMem);
          break;
        case LZOMethod.M1x999:
          ret = lzo1x_999_compress(src, srcLen, dst, dstLen, wrkMem);
          break;
        default:
          throw new ArgumentException("Unknow method: " + method);
      }
      if (ret != 0)
        throw new LZOException(ret);
    }

    public static void Decompress(LZOMethod method, byte* src, IntPtr srcLen, byte* dst, IntPtr* dstLen)
    {
      int ret;
      switch (method)
      {
        case LZOMethod.M1x15:
        case LZOMethod.M1x999:
          ret = lzo1x_decompress(src, srcLen, dst, dstLen, null);
          break;
        default:
          throw new ArgumentException("Unknow method: " + method);
      }
      if (ret != 0)
        throw new LZOException(ret);
    }

    public class LZOException : Exception
    {
      private int _code;
      internal LZOException(int code) { _code = code; }
      public override string Message { get { return MESSAGES[_code]; } }

      private static Dictionary<int, string> MESSAGES = new Dictionary<int, string>()
      {
        {-1,  "Error"},
        {-2,  "Out of memory"},
        {-3,  "Not compressible"},
        {-4,  "Input overrun"},
        {-5,  "Output overrun"},
        {-6,  "Lookbehind overrun"},
        {-7,  "EOF not found"},
        {-8,  "Input not consumed"},
        {-9,  "Not yet implemented"},
        {-10, "Invalid argument"}
      };
    }
  }
}